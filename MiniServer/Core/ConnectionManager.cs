﻿#define DEBUG

using System.Collections.Concurrent;
using System.Diagnostics;
using Grpc.Core;
using MiniProtoImpl;
using MiniServer.Core.Events;
using MiniServer.Data.DTO;
using MiniServer.Data.Model;
using MiniServer.Services;
using Npgsql.Replication.PgOutput.Messages;
using Message = MiniProtoImpl.Message;

namespace MiniServer.Core {

    public interface IConnectionManager {
        Task HandleConnectedUser(UserConnection instream);
        Task HandlePipesUpdate();
        IEnumerable<UserConnection>? GetOpenedConnection(long msgReceiverId);
        Task HandleGroupCreated(Group group);
    }

    public class ConnectionManager : IConnectionManager {
        private readonly int _pipeEmptyAmount;
        private readonly int _pipeEmptyDelayMs;
        private readonly int _pipeUnloadedAmount;
        private readonly int _pipeUnloadedDelayMs;
        private static int GetEnvironmentVariableInt(string variableName, int defaultValue)
        {
            string? value = Environment.GetEnvironmentVariable(variableName);
            return string.IsNullOrEmpty(value) ? defaultValue : int.Parse(value);
        }

        private const int DefaultPipeEmptyAmount = 0;
        private const int DefaultPipeEmptyDelayMs = 150;
        private const int DefaultPipeUnloadedAmount = 4;
        private const int DefaultPipeUnloadedDelayMs = 100;
        
        private readonly IAuthenticationService _authenticationService;
        private readonly EventDispatcher _eventDispatcher;
        private readonly ILogger<ConnectionManager> _logger;

        private readonly BlockingCollection<UserConnection> _connectedUsers;

        private readonly ConnectedUsersRegistry _registry;

        private readonly ICommEventFactory _commEventFactory;
        
        public ConnectionManager(IAuthenticationService authenticationService,
            EventDispatcher eventDispatcher, ILogger<ConnectionManager> logger, ICommEventFactory commEventFactory) {
            _authenticationService = authenticationService;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
            _commEventFactory = commEventFactory;
            _registry = new ConnectedUsersRegistry();
            _connectedUsers = new BlockingCollection<UserConnection>();
            logger.LogInformation("Connection service started.");

            
            _pipeEmptyAmount = GetEnvironmentVariableInt("MINISERVER_PIPE_EMPTY_AMOUNT", DefaultPipeEmptyAmount);
            _pipeEmptyDelayMs = GetEnvironmentVariableInt("MINISERVER_PIPE_EMPTY_DELAY_MS", DefaultPipeEmptyDelayMs);
            _pipeUnloadedAmount = GetEnvironmentVariableInt("MINISERVER_PIPE_UNLOADED_AMOUNT", DefaultPipeUnloadedAmount);
            _pipeUnloadedDelayMs = GetEnvironmentVariableInt("MINISERVER_PIPE_UNLOADED_DELAY_MS", DefaultPipeUnloadedDelayMs);

            
            
            _eventDispatcher.EnqueueEvent(HandlePipesUpdate);
        }

        public Task HandleConnectedUser(UserConnection instream) {
            MonitorConnection(instream);
            _connectedUsers.Add(instream);
            instream.SetRegistry(_registry);
            return Task.CompletedTask;
        }

        public async Task HandlePipesUpdate() {
            try {
#if DEBUG
                _logger.LogInformation("Handling pipes" + DateTime.Now);
#endif
                foreach (var user in _connectedUsers) {
                    await HandleUserPipe(user);
                }
                var eventsInQueue = _eventDispatcher.GetLoad();
            
                if (eventsInQueue <= _pipeEmptyAmount)
                {
                    await Task.Delay(_pipeEmptyDelayMs);
                }
                else if (eventsInQueue < _pipeUnloadedAmount)
                {
                    await Task.Delay(_pipeUnloadedDelayMs);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }


            // Enqueue the next HandlePipesUpdate event
            _eventDispatcher.EnqueueEvent(HandlePipesUpdate);
        }
        private async void MonitorConnection(UserConnection userCon)
        {
            try
            {
                // Await the cancellation token to be triggered
                await Task.Run(() => userCon.Context.CancellationToken.WaitHandle.WaitOne());
            
                // Handle disconnection
                _connectedUsers.TryTake(out _);
                userCon.Dispose();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                // Handle the RpcException if the call is cancelled
                _connectedUsers.TryTake(out _);
                userCon.Dispose();
            }
        }

        public IEnumerable<UserConnection>? GetOpenedConnection(long msgReceiverId) {
            return _registry.GetUserConnections(msgReceiverId);
        }

        public Task HandleGroupCreated(Group group) {
            var members = group.GroupRoles.Select(role => role.UserId).ToList();
            var userConnections = new List<UserConnection>();
            foreach (var member in members) {
                var connections = _registry.GetUserConnections(member);
                if (connections != null) {
                    userConnections.AddRange(connections);
                }
            }
            var response = new CommunicationResponse {
                CreateGroup = new CreateGroupResponse {
                    Id = group.GroupId,
                    Name = group.Name,
                    IsSucceed = true,
                    AuthorId = group.CreatorUserId
                }
            };
            foreach (var userConnection in userConnections) {
                userConnection.ResponseStream.WriteAsync(response);
            }
            return Task.CompletedTask;
        }
        
        

        private async Task HandleUserPipe(UserConnection user) {
            try {
                await foreach (var msg in user.Reader) {
                    _logger.LogInformation($"Received message: {msg}");
                    ProcessRequest(msg, user);
                }


            }
            catch (Exception e) {
                _logger.LogError($"Error handling user pipe: {e}");
            }

        }

        private void ProcessRequest(CommunicationRequest msg, UserConnection user) {
            var token = msg.Token;
            var identity = _authenticationService.GetIdentity(token);
            var userId = identity.GetUserId();
            var isAuthenticated = identity.IsAuthenticated();
            if (!isAuthenticated) {
                _logger.LogWarning("Unauthorized request");
                user.ReportUnauthorized();
                return;
            }
            UserConnection.UpdateUser(user, userId, token);

            switch (msg.ContentCase)
            {
                case CommunicationRequest.ContentOneofCase.None:
                    break;
                case CommunicationRequest.ContentOneofCase.Message:
                    {
                        var evnt = _commEventFactory.Create<MessageSentEvent, AuthorizedRequest<Message>>(
                            new AuthorizedRequest<Message>(Convert.ToInt64(userId), msg.Message, user),
                            () => _logger.LogInformation($"Message sent by {userId}"));
                        var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<Message>>();
                        _eventDispatcher.EnqueueEvent(async () =>
                        {
                            await evnt.Execute(taskCompletionSource);
                        });
                        break;
                    }
                // case CommunicationRequest.ContentOneofCase.DeleteMessage:
                //     {
                //         var evnt = _commEventFactory.Create<DeleteMessageEvent, AuthorizedRequest<DeleteMessageRequest>>(
                //             new AuthorizedRequest<DeleteMessageRequest>(Convert.ToInt64(userId), msg.DeleteMessage, user),
                //             () => _logger.LogInformation($"Message deleted by {userId}"));
                //         var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<DeleteMessageRequest>>();
                //         _eventDispatcher.EnqueueEvent(async () =>
                //         {
                //             await evnt.Execute(taskCompletionSource);
                //         });
                //         break;
                //     }
                case CommunicationRequest.ContentOneofCase.RequestUpdate:
                    {
                        var evnt = _commEventFactory.Create<RequestUpdateEvent, AuthorizedRequest<RequestUpdate>>(
                            new AuthorizedRequest<RequestUpdate>(Convert.ToInt64(userId), msg.RequestUpdate, user),
                            () => _logger.LogInformation($"Request update by {userId}"));
                        var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<RequestUpdate>>();
                        _eventDispatcher.EnqueueEvent(async () =>
                        {
                            await evnt.Execute(taskCompletionSource);
                        });
                        break;
                    }
                case CommunicationRequest.ContentOneofCase.RequestDialog:
                    {
                        var evnt = _commEventFactory.Create<RequestDialogEvent, AuthorizedRequest<RequestDialog>>(
                            new AuthorizedRequest<RequestDialog>(Convert.ToInt64(userId), msg.RequestDialog, user),
                            () => _logger.LogInformation($"Request dialog by {userId}"));
                        var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<RequestDialog>>();
                        _eventDispatcher.EnqueueEvent(async () =>
                        {
                            await evnt.Execute(taskCompletionSource);
                        });
                        break;
                    }
                case CommunicationRequest.ContentOneofCase.CreateGroup: 
                {
                    var evnt = _commEventFactory.Create<CreateGroupEvent, AuthorizedRequest<CreateGroupRequest>>(
                        new AuthorizedRequest<CreateGroupRequest>(Convert.ToInt64(userId), msg.CreateGroup, user),
                        () => _logger.LogInformation($"Group created by {userId}"));
                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<CreateGroupRequest>>();
                    _eventDispatcher.EnqueueEvent(async () =>
                    {
                        await evnt.Execute(taskCompletionSource);
                    });

                    break;
                    
                }
                case CommunicationRequest.ContentOneofCase.DeleteGroup: 
                {
                    var evnt = _commEventFactory.Create<DeleteGroupEvent, AuthorizedRequest<DeleteGroupRequest>>(
                        new AuthorizedRequest<DeleteGroupRequest>(Convert.ToInt64(userId), msg.DeleteGroup, user),
                        () => _logger.LogInformation($"Group deleted by {userId}"));
                    
                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<DeleteGroupRequest>>();
                    _eventDispatcher.EnqueueEvent(async () =>
                    {
                        await evnt.Execute(taskCompletionSource);
                    });
                    
                    
                    break;
                }
                case CommunicationRequest.ContentOneofCase.AddMember: {
                    var evnt = _commEventFactory.Create<AddMemberEvent, AuthorizedRequest<AddMemberRequest>>(
                        new AuthorizedRequest<AddMemberRequest>(Convert.ToInt64(userId), msg.AddMember, user),
                        () => _logger.LogInformation($"Member added by {userId}"));
                    
                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<AddMemberRequest>>();
                    _eventDispatcher.EnqueueEvent(async () =>
                    {
                        await evnt.Execute(taskCompletionSource);
                    });
                    
                    break;
                }
                case CommunicationRequest.ContentOneofCase.RemoveMember: {
                    var evnt = _commEventFactory.Create<RemoveMemberEvent, AuthorizedRequest<RemoveMemberRequest>>(
                        new AuthorizedRequest<RemoveMemberRequest>(Convert.ToInt64(userId), msg.RemoveMember, user),
                        () => _logger.LogInformation($"Member removed by {userId}"));
                    
                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<RemoveMemberRequest>>();
                    _eventDispatcher.EnqueueEvent(async () =>
                    {
                        await evnt.Execute(taskCompletionSource);
                    });
                    
                    break;
                }
                
                case CommunicationRequest.ContentOneofCase.Search: {


                    var evnt = _commEventFactory.Create<SearchEvent, AuthorizedRequest<SearchRequest>>(
                        new AuthorizedRequest<SearchRequest>(Convert.ToInt64(userId), msg.Search, user),
                        () => _logger.LogInformation($"Search by {userId}"));

                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<SearchRequest>>();
                    _eventDispatcher.EnqueueEvent(async () => { await evnt.Execute(taskCompletionSource); });

                    break;
                }

                case CommunicationRequest.ContentOneofCase.FetchUsers: {
                    var evnt = _commEventFactory.Create<FetchUsersEvent, AuthorizedRequest<FetchUserInfoRequest>>(
                        new AuthorizedRequest<FetchUserInfoRequest>(Convert.ToInt64(userId), msg.FetchUsers, user),
                        () => _logger.LogInformation($"Fetch users by {userId}"));
                    
                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<FetchUserInfoRequest>>();
                    
                    _eventDispatcher.EnqueueEvent(async () =>
                    {
                        await evnt.Execute(taskCompletionSource);
                    });
                    
                    break;
                }
                case CommunicationRequest.ContentOneofCase.SetPersonStatus: {
                    var evnt = _commEventFactory.Create<SetUserStatusEvent, AuthorizedRequest<SetPersonStatus>>(
                        new AuthorizedRequest<SetPersonStatus>(Convert.ToInt64(userId), msg.SetPersonStatus, user),
                        () => _logger.LogInformation($"Set person status by {userId}"));
                    var taskCompletionSource = new TaskCompletionSource<AuthorizedRequest<SetPersonStatus>>();
                    _eventDispatcher.EnqueueEvent(async () =>
                    {
                        await evnt.Execute(taskCompletionSource);
                    });
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
    }




    internal struct UserCreds {
        public string UserId;
        public string Token;
    }
    public class UserConnection {
        public readonly IServerStreamWriter<CommunicationResponse> ResponseStream;
        public readonly ServerCallContext Context;
        public readonly TaskCompletionSource<bool> Tcs = new TaskCompletionSource<bool>(false);
        public readonly IAsyncEnumerable<CommunicationRequest> Reader;
        private UserCreds _userCreds = new UserCreds();
        private ConnectedUsersRegistry? _registry;
        public UserConnection(IAsyncStreamReader<CommunicationRequest> requestStream, IServerStreamWriter<CommunicationResponse> responseStream, ServerCallContext context) {
            ResponseStream = responseStream;
            Context = context;
            Reader = requestStream.ReadAllAsync();

        }
        ~UserConnection() {
            Tcs.SetResult(true);
            _registry?.RemoveUserConnection(this, _userCreds.UserId);
        }

        private static long? ConvertToLong(string? userId) { // HELPER METHOD
            return userId == null ? null : Convert.ToInt64(userId);
        }
        public static void UpdateUser(UserConnection user, string? userId, string token) {
            user._userCreds.UserId = userId ?? string.Empty;
            user._userCreds.Token = token;
            user._registry?.AddUserConnection(user, ConvertToLong(userId));
        }

        public void ReportUnauthorized() {
            var response = new CommunicationResponse {
                ErrorMsg = "Unauthorized request, please update your credentials."
            };
            ResponseStream.WriteAsync(response);
        }

        internal void SetRegistry(ConnectedUsersRegistry? registry) {
            _registry = registry;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is UserConnection user) {
                return user.GetHashCode() == GetHashCode();
            }

            return false;
        }
        
        public override int GetHashCode() // TODO fix the bag
        {
            return _userCreds.UserId?.GetHashCode() ?? 0;
        }

        public void Dispose() {
            Tcs.SetResult(true);
            _registry?.RemoveUserConnection(this, _userCreds.UserId);
            
        }
    }

    internal class ConnectedUsersRegistry
    {
        private readonly ConcurrentDictionary<long, ConcurrentBag<UserConnection>> _users = new ConcurrentDictionary<long, ConcurrentBag<UserConnection>>();

        // Add or update a user connection
        public void AddUserConnection(UserConnection user, long? userId)
        {
            if (userId == null)
            {
                return;
            }
            var key = userId??0;
            _users.AddOrUpdate(key, new ConcurrentBag<UserConnection> { user }, (k, v) => v);
            // _users.TryUpdate(key, new ConcurrentBag<UserConnection> { user });
            //     , (k, v) =>
            // {
            //     v.Add(user);
            //     return v;
            // });
        }

        // Retrieve all user connections by userId
        public IEnumerable<UserConnection>? GetUserConnections(long userId)
        {
            return _users.TryGetValue(userId, out var users) ? users : null;
        }

        // Remove a specific user connection
        public bool RemoveUserConnection(UserConnection user, string userId)
        {
            long key = Convert.ToInt64(userId);
            if (_users.TryGetValue(key, out var users))
            {
                var userList = users.ToList();
                bool removed = userList.Remove(user);
                if (removed)
                {
                    if (userList.Any())
                    
                    {
                        _users[key] = new ConcurrentBag<UserConnection>(userList);
                    }
                    else
                    {
                        _users.TryRemove(key, out _);
                    }
                }
                return removed;
            }
            return false;
        }

        // Remove all user connections by userId
        public bool RemoveAllUserConnections(string userId)
        {
            return _users.TryRemove(Convert.ToInt64(userId), out _);
        }
    }
}


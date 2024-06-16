using Grpc.Core;
using Microsoft.Extensions.Logging;
using MiniServer.Events;
using MiniServer.Services;
using System.Threading.Tasks;
using MiniServer.Core;

namespace MiniServer.Services
{
    public class ChatService : Chat.ChatBase {
        private readonly ILogger<ChatService> _logger;
        private readonly EventDispatcher _eventDispatcher;
        private readonly IChatLogicService _chatLogicService;
        private readonly IAuthenticationService _authenticationService;

        public ChatService(ILogger<ChatService> logger, EventDispatcher eventDispatcher,
            IChatLogicService chatLogicService, IAuthenticationService authenticationService) {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _chatLogicService = chatLogicService;
            _authenticationService = authenticationService;
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context) {
            var registerEvent = new RegisterEvent(request, _chatLogicService,
                () => { _logger.LogInformation($"Registering user {request.Credentials.Name}"); });

            // Enqueue the event to be processed asynchronously
            var taskCompletionSource = new TaskCompletionSource<RegisterResponse>();

            _eventDispatcher.EnqueueEvent(async () => {
                try {
                    var response = await registerEvent.Execute();
                    taskCompletionSource.SetResult(response);
                }
                catch (Exception ex) {
                    taskCompletionSource.SetException(ex);
                }
            });

            // Return the task completion source's task
            return taskCompletionSource.Task;
        }

        public override Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context) {
            var connectEvent = new ConnectEvent(request, _chatLogicService,
                () => { _logger.LogInformation($"Connecting user {request.Credentials.Name}"); });
            var taskCompletionSource = new TaskCompletionSource<ConnectResponse>();

            _eventDispatcher.EnqueueEvent(async () => {
                    try {
                        var response = await connectEvent.Execute();
                        taskCompletionSource.SetResult(response);
                    }
                    catch (Exception ex) {
                        taskCompletionSource.SetException(ex);
                    }
                }
            );
            return taskCompletionSource.Task;
        }

        public override Task<ConnectResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context) {
            var device = request.Device;
            
            var refreshTokenEvent = new RefreshTokenEvent(request, _authenticationService,
                () => { _logger.LogInformation($"Refreshing token for user {request.Name}"); });
            var taskCompletionSource = new TaskCompletionSource<ConnectResponse>();

            _eventDispatcher.EnqueueEvent(async () => {
                    try {
                        var response = await refreshTokenEvent.Execute();
                        taskCompletionSource.SetResult(response);
                    }
                    catch (Exception ex) {
                        taskCompletionSource.SetException(ex);
                    }
                }
            );
            return taskCompletionSource.Task;
        }

    }
}
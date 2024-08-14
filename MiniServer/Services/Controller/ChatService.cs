using Grpc.Core;
using MiniProtoImpl;
using MiniServer.Core;
using MiniServer.Core.Events;

namespace MiniServer.Services.Controller; 

public class ChatService : Chat.ChatBase {
    private readonly ILogger<ChatService> _logger;
    private readonly EventDispatcher _eventDispatcher;
    private readonly ICommEventFactory _commEventFactory;

    public ChatService(ILogger<ChatService> logger, EventDispatcher eventDispatcher,
        ICommEventFactory commEventFactory) {
        _logger = logger;
        _eventDispatcher = eventDispatcher;
        _commEventFactory = commEventFactory;
    }
    private Task<TResponse> HandleEventAsync<TResponse>(EventBase<TResponse> eventBase, Action? logAction = null)
    {
        TaskCompletionSource<TResponse> taskCompletionSource = new TaskCompletionSource<TResponse>();

        _eventDispatcher.EnqueueEvent(async () =>
        {
            try
            {
                await eventBase.Execute(taskCompletionSource);
            }
            catch (Exception e)
            {
                taskCompletionSource.SetException(e);
            }
        });

        return taskCompletionSource.Task;
    }

    /// <summary>
    /// Add new user to database
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context">Implicit parameter</param>
    /// <returns> If successful: session token, refresh token </returns>
    public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        // Use the factory to create the event
        RegisterEvent registerEvent = _commEventFactory.Create<RegisterEvent, RegisterResponse>(request, 
            () => _logger.LogInformation($"Registering user {request.Credentials.Name}"));

        return HandleEventAsync(registerEvent);
    }

    /// <summary>
    /// Log in an existing user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns> If successful: session token, refresh token </returns>
    public override Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
    {
        // Use the factory to create the event
        var connectEvent = _commEventFactory.Create<ConnectEvent, ConnectResponse>(request,
            () => _logger.LogInformation($"Connecting user {request.Credentials.Name}"));

        return HandleEventAsync(connectEvent);
    }

    /// <summary>
    /// Get a new session token when the previous session token is either close to expiring or has already expired
    /// </summary>
    /// <param name="request"> Contains the user, device and refresh token </param>
    /// <param name="context"></param>
    /// <returns> New session token </returns>
    public override Task<ConnectResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
    {
        // Use the factory to create the event
        var refreshTokenEvent = _commEventFactory.Create<RefreshTokenEvent, ConnectResponse>(request,
            () => _logger.LogInformation($"Refreshing token for user {request.Name}"));

        return HandleEventAsync(refreshTokenEvent);
    }

    /// <summary>
    /// Use this at the beginning of program
    /// Creates an async channel for the client to receive messages from the server
    /// </summary>
    /// <param name="requestStream"> Reference to async stream used for client to server communication </param>
    /// <param name="responseStream"> Reference to async stream used for server to client communication </param>
    /// <param name="context"></param>
    /// <returns>Object containing the request and response stream</returns>
    public override Task InitiateAsyncChannel(IAsyncStreamReader<CommunicationRequest> requestStream, IServerStreamWriter<CommunicationResponse> responseStream,
        ServerCallContext context) {
        UserConnection userConnection = new  UserConnection(requestStream, responseStream, context);
        UserConnectedEvent userConnectedEvent = _commEventFactory.Create<UserConnectedEvent, UserConnection>(userConnection,
            () => _logger.LogInformation($"User connected: {context.Peer}"));
            
        HandleEventAsync(userConnectedEvent);
            
        return userConnection.Tcs.Task;
    }
}
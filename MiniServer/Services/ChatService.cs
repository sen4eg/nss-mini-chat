using Grpc.Core;
using Microsoft.Extensions.Logging;
using MiniServer.Services;
using System.Threading.Tasks;
using Google.Protobuf;
using MiniServer.Core;
using MiniServer.Core.Events;

namespace MiniServer.Services
{
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

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            // Use the factory to create the event
            RegisterEvent registerEvent = _commEventFactory.Create<RegisterEvent, RegisterResponse>(request, 
                () => _logger.LogInformation($"Registering user {request.Credentials.Name}"));

            return HandleEventAsync(registerEvent);
        }

        public override Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            // Use the factory to create the event
            var connectEvent = _commEventFactory.Create<ConnectEvent, ConnectResponse>(request,
                () => _logger.LogInformation($"Connecting user {request.Credentials.Name}"));

            return HandleEventAsync(connectEvent);
        }

        public override Task<ConnectResponse> RefreshToken(RefreshTokenRequest request, ServerCallContext context)
        {
            // Use the factory to create the event
            var refreshTokenEvent = _commEventFactory.Create<RefreshTokenEvent, ConnectResponse>(request,
                () => _logger.LogInformation($"Refreshing token for user {request.Name}"));

            return HandleEventAsync(refreshTokenEvent);
        }

    }
}
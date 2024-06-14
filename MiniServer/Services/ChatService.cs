using Grpc.Core;
using Microsoft.Extensions.Logging;
using MiniServer.Events;
using MiniServer.Services;
using System.Threading.Tasks;
using MiniServer.Core;

namespace MiniServer.Services
{
    public class ChatService : Chat.ChatBase
    {
        private readonly ILogger<ChatService> _logger;
        private readonly EventDispatcher _eventDispatcher;
        private readonly IChatLogicService _chatLogicService;

        public ChatService(ILogger<ChatService> logger, EventDispatcher eventDispatcher, IChatLogicService chatLogicService)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
            _chatLogicService = chatLogicService;
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context) {
            // var dummyResponse = new RegisterResponse
            // {
            //     IsSucceed = true,
            //     ErrorMsg = "",
            //     Token = "dummy-token",
            //     RefreshToken = "dummy-refresh-token"
            // };
            //
            var registerEvent = new RegisterEvent(request, _chatLogicService, () =>
            {
                _logger.LogInformation($"Registering user {request.Name}");
            });

            // Enqueue the event to be processed asynchronously
            var taskCompletionSource = new TaskCompletionSource<RegisterResponse>();
            
            _eventDispatcher.EnqueueEvent(async () =>
            {
                try
                {
                    var response = await registerEvent.Execute();
                    taskCompletionSource.SetResult(response);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            // Return the task completion source's task
            return taskCompletionSource.Task;
        }
    }
}
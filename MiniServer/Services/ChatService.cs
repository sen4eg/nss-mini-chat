using Grpc.Core;

namespace MiniServer.Services
{
    public class ChatService : Chat.ChatBase
    {
        private readonly ILogger<GreeterService> _logger;
        public ChatService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        // public ChatService Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        // {
        //     return Task.FromResult(new HelloReply
        //     {
        //         Message = "Hello " + request.Name
        //     });
        // }
        
        public Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            // TODO: Implement the Register method
            return Task.FromResult(new RegisterResponse
            {
                IsSucceed = true
            });
        }
    }
}
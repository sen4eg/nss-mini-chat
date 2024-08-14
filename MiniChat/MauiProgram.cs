using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using MiniChat.ViewModel;
using MiniProtoImpl;
namespace MiniChat
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            // --------- ADD PAGES -----------
            builder.Services.AddSingleton<LoginPage>();
            builder.Services.AddSingleton<LoginViewModel>();
            builder.Services.AddSingleton<ConversationSelectionPage>();
            builder.Services.AddSingleton<ConversationSelectionViewModel>();

            builder.Services.AddTransient<ConversationPage>();
            builder.Services.AddTransient<ConversationViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
        struct ConnectionObj
        {
            public IAsyncEnumerable<CommunicationResponse> reader;
            public GrpcChannel channel;
            public IClientStreamWriter<CommunicationRequest> writer;
        }

        // either everywhere MiniServer.smthing or using MiniServer at top
        public static async void ChatExample()
        {
            // init
            using var channel = GrpcChannel.ForAddress("https://localhost:5422"); // make sure to use right port, maybe add posibility to set it along with ip for development stage
            var client = new MiniProtoImpl.Chat.ChatClient(channel);

            // request creation
            var req = new RegisterRequest
            {
                Credentials = new Credentials
                {
                    Name = "someUsername",
                    Password = "password123"
                },
                Device = new MiniProtoImpl.Device // maui also have device so we specifically want the grpc one
                {
                    Ip = "127.0.0.1",
                    Name = "SomePC",
                    Os = "Windows"
                },
                Email = "example@mail.com"
            };

            // synchronous
            var response = client.Register(req);
            Console.WriteLine(response); // of type RegisterResponse
            var _1hour_auth_token = response.Token;  // auth token for hour
            var _keep_long_refresh_token = response.RefreshToken; // refresh token eternal 
            var _if_not_empty_something_wrong_error_msg = response.ErrorMsg;

            // async option
            var responsePromise = await client.RegisterAsync(req);


            // Opens our lovely lightspeed two way communication channel
            var comchannel = client.InitiateAsyncChannel();

            var reqstream = comchannel.RequestStream; // Throw you input here
            var respstream = comchannel.ResponseStream; // Server data will pop out of here
            // Note that it's pretty asynchronous process so if you ordered lets say multiple packs of messages in some order they would arive in whichever order server processes them

            var communicationRequest = new CommunicationRequest
            {
                Token = _1hour_auth_token, // "auth_token from previous step",
                Message = new Message
                {
                    ReceiverId = 2, // should be negative for groups, since groups have negative id
                    Content = "Hello there!"
                }
            };


            var newmsg = new MiniProtoImpl.Message
            {
                Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
            };
            // Todo for later check if this is not broken frontend side
            var date = newmsg.Timestamp.ToDateTime();

            await reqstream.WriteAsync(communicationRequest);

            // init, keep it somewhere along with channel
            var reader = respstream.ReadAllAsync();



            await foreach (var entry in reader)
            {
                // process entry of type CommunicationResponse
            }

            channel.Dispose(); // dispose at end to ensure ports are freed

            ConnectionObj obj = new ConnectionObj
            {
                reader = reader,
                writer = reqstream,
                channel = channel
            }; // You'll have some class alike this to store connection to server

            // connection data is to be stored for time of client app is open
        }
    }
}


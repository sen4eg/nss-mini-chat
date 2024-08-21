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
            builder.Services.AddSingleton<AddConversationPage>();
            builder.Services.AddSingleton<AddConversationViewModel>();

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
    }
}


using Microsoft.EntityFrameworkCore;
using MiniServer.Core;
using MiniServer.Data;
using MiniServer.Data.Repository;
using MiniServer.Services;
using MiniServer.Services.Controller;

namespace MiniServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Config along with surprisingly "bean" type definition
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddDbContext<ChatContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("ChatContext"));
            }, ServiceLifetime.Singleton);
            
            builder.Services.AddSingleton<ICommEventFactory, CommEventFactory>();
            builder.Services.AddSingleton<EventDispatcher>();
            builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

            
            builder.Services.AddTransient<IConnectionLogicService, ConnectionLogicService>(); // Register the business logic service
            builder.Services.AddTransient<IAuthenticationService, AuthenticationService>(); // Register the business logic service
            builder.Services.AddTransient<ChatService>(); // Register the gRPC service
            builder.Services.AddTransient<IGroupService, GroupService>();
            
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IMessageRepository, MessageRepository>();
            builder.Services.AddTransient<IContactRepository, ContactRepository>();
            builder.Services.AddTransient<IGroupRepository, GroupRepository>();
            builder.Services.AddTransient<IValidationTokenRepository, ValidationTokenRepository>();
            
            builder.Services.AddTransient<IMessagingService, MessagingService>();
            builder.Services.AddTransient<IPersistenceService, PersistenceService>();
            builder.Services.AddTransient<IContactService, ContactService>();
            builder.Services.AddTransient<ISearchService, SearchService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ChatContext>();
                context.Database.Migrate();
            }

            var dispatcher = app.Services.GetRequiredService<EventDispatcher>();
            dispatcher.Start(); // Start the dispatcher

            dispatcher.EnqueueEvent(() => {
                Console.WriteLine("Hello from the dispatcher!");
                return Task.CompletedTask;
            });
            // Configure the HTTP request pipeline.
            app.MapGrpcService<ChatService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            // Ensure the dispatcher is stopped when the application shuts down
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                dispatcher.Stop();
            });

            app.Run();
        }
    }
}
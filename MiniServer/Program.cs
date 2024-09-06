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
            builder.Services.AddPooledDbContextFactory<ChatContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("ChatContext"));
                // options.AddInterceptors();
                options.EnableSensitiveDataLogging(); // Если нужно
            }, poolSize: 8);
            
            // builder.Services.AddDbContext<ChatContext>(options =>
            // {
            //     options.UseNpgsql(builder.Configuration.GetConnectionString("ChatContext"));
            // }, ServiceLifetime.Scoped);
            
            builder.Services.AddSingleton<ICommEventFactory, CommEventFactory>();
            builder.Services.AddSingleton<EventDispatcher>();
            builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
            
            builder.Services.AddTransient<IConnectionLogicService, ConnectionLogicService>(); // Register the business logic service
            builder.Services.AddTransient<IAuthenticationService, AuthenticationService>(); // Register the business logic service
            builder.Services.AddTransient<ChatService>(); // Register the gRPC service
            builder.Services.AddTransient<IGroupService, GroupService>();
            
            builder.Services.AddScoped<IUserRepository>(provider =>
            {
                var factory = provider.GetRequiredService<IDbContextFactory<ChatContext>>();
                return new UserRepository(factory.CreateDbContext());
            });
            builder.Services.AddScoped<IMessageRepository, MessageRepository>(provider =>
            {
                var factory = provider.GetRequiredService<IDbContextFactory<ChatContext>>();
                return new MessageRepository(factory.CreateDbContext());
            });
            builder.Services.AddScoped<IContactRepository, ContactRepository>(provider => {
                var factory = provider.GetRequiredService<IDbContextFactory<ChatContext>>();
                return new ContactRepository(factory.CreateDbContext());
            });

            builder.Services.AddScoped<IGroupRepository, GroupRepository>(provider => {
                var factory = provider.GetRequiredService<IDbContextFactory<ChatContext>>();
                return new GroupRepository(factory.CreateDbContext());
            });

            builder.Services.AddScoped<IValidationTokenRepository, ValidationTokenRepository>(provider => {
                var factory = provider.GetRequiredService<IDbContextFactory<ChatContext>>();
                return new ValidationTokenRepository(factory.CreateDbContext());
            });
            
            builder.Services.AddTransient<IMessagingService, MessagingService>();
            builder.Services.AddTransient<IPersistenceService, PersistenceService>();
            builder.Services.AddTransient<IContactService, ContactService>();
            builder.Services.AddTransient<ISearchService, SearchService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContextFactory = services.GetRequiredService<IDbContextFactory<ChatContext>>();
                using (var context = dbContextFactory.CreateDbContext())
                {
                    context.Database.Migrate();
                }
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
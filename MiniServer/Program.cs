using Microsoft.EntityFrameworkCore;
using MiniServer.Data;
using MiniServer.Services;

namespace MiniServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddDbContext<ChatContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("ChatContext"));
            });
            

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ChatContext>();
                context.Database.Migrate();
            }
            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            app.Run();
        }
    }
}
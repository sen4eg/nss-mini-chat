using Grpc.Core;
using System.Threading.Tasks;
using MiniProtoImpl;
using MiniServer.Core;
using MiniServer.Data.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace MiniServer.Services
{
    public interface IConnectionLogicService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<ConnectResponse> ConnectAsync(ConnectRequest request);
    }

    public class ConnectionLogicService : IConnectionLogicService {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ConnectionLogicService(IServiceScopeFactory serviceScopeFactory){
            _serviceScopeFactory = serviceScopeFactory;   
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                // Resolve scoped dependencies
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var authenticationService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();

                // Check if the user already exists
                if (await userRepository.UserExistsAsync(request.Email, request.Credentials.Name))
                {
                    return new RegisterResponse
                    {
                        IsSucceed = false,
                        ErrorMsg = "User with such username or email already exists."
                    };
                }

                // Validate password
                if (!IsValidPassword(request.Credentials.Password))
                {
                    return new RegisterResponse
                    {
                        IsSucceed = false,
                        ErrorMsg = "Password does not meet the required criteria."
                    };
                }

                // Create user in the repository
                var user = await userRepository.CreateUserAsync(request.Credentials.Name, request.Email, request.Credentials.Password);

                // Generate tokens
                var token = authenticationService.GenerateToken(user.UserId);
                var refreshToken = authenticationService.GenerateRefreshToken(request.Credentials.Name, request.Device);

                var response = new RegisterResponse
                {
                    IsSucceed = true,
                    Token = token,
                    RefreshToken = refreshToken
                };

                return response;
            }
        }

        public async Task<ConnectResponse> ConnectAsync(ConnectRequest request) 
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var authenticationService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
                return await authenticationService.Authenticate(request);
            }
        }

        private bool IsValidPassword(string password)
        {
            return password.Length > 6; // Simple length check for now
        }
    }
}

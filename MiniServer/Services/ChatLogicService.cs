using Grpc.Core;
using System.Threading.Tasks;
using MiniServer.Core;
using MiniServer.Data.Repository;
using MiniServer.Events;

namespace MiniServer.Services
{
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public interface IChatLogicService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        // Add other methods as needed
        Task<ConnectResponse> ConnectAsync(ConnectRequest request);
    }

    public class ChatLogicService : IChatLogicService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;

        public ChatLogicService(IUserRepository userRepository, IAuthenticationService authenticationService)
        {
            _userRepository = userRepository;
            _authenticationService = authenticationService;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if the user already exists
            if (await _userRepository.UserExistsAsync(request.Email, request.Credentials.Name))
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
            await _userRepository.CreateUserAsync(request.Credentials.Name, request.Email, request.Credentials.Password);

            // Generate tokens
            var token = _authenticationService.GenerateToken(request.Credentials.Name);
            var refreshToken = _authenticationService.GenerateRefreshToken(request.Credentials.Name, request.Device);

            var response = new RegisterResponse
            {
                IsSucceed = true,
                Token = token,
                RefreshToken = refreshToken
            };

            return response;
        }

        public Task<ConnectResponse> ConnectAsync(ConnectRequest request) {
            return _authenticationService.Authenticate(request);
        }

        private bool IsValidPassword(string password)
        {
            return password.Length > 6; // Simple length check for now
        }

    }

}
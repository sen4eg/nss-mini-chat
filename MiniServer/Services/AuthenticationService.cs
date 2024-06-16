using Microsoft.IdentityModel.Tokens;
using MiniServer.Data.Repository;
using MiniServer.Utils;

namespace MiniServer.Services; 

public interface IAuthenticationService
{
    string GenerateToken(string name);
    string GenerateRefreshToken(string username, Device device);
    Task<ConnectResponse> RefreshTokenAsync(RefreshTokenRequest request);
    // Add other methods as needed
    Task<ConnectResponse> Authenticate(ConnectRequest request);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IValidationTokenRepository _authenticationRepository;
    private readonly IUserRepository _userRepository;

    public AuthenticationService(IValidationTokenRepository authenticationRepository, IUserRepository userRepository)
    {
        _authenticationRepository = authenticationRepository;
        _userRepository = userRepository;
    }
    public string GenerateToken(string username)
    {
        return TokenHelper.GenerateAccessToken(username);
    }

    public string GenerateRefreshToken(string name) {
        return TokenHelper.GenerateRefreshToken();
    }

    public string GenerateRefreshToken(string username, Device device)
    {
        var refresh = TokenHelper.GenerateRefreshToken();
        _authenticationRepository.StoreToken(username, device, refresh);
        return refresh;
    }

    public async Task<ConnectResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (!await ValidateRefreshToken(request))
        {
            return new ConnectResponse
            {
                IsSucceed = false,
            };
        }

        return new ConnectResponse
        {
            IsSucceed = true,
            Token = GenerateToken(request.Name),
            RefreshToken = request.RefreshToken
        };
    }

    public async Task<ConnectResponse> Authenticate(ConnectRequest request) {
        var token = await _authenticationRepository.FindToken(request.Credentials.Name, request.Device);
        var credsValid = await VerifyCredentials(request.Credentials);
        if (credsValid) {
            if (!string.IsNullOrEmpty(token)) {
                return new ConnectResponse {
                    IsSucceed = true,
                    Token = GenerateToken(request.Credentials.Name),
                    RefreshToken = token // Reuse the existing refresh token on same device
                };
            }
            return new ConnectResponse {
                IsSucceed = true,
                Token = GenerateToken(request.Credentials.Name),
                RefreshToken = GenerateRefreshToken(request.Credentials.Name, request.Device)
            };
        }
        return new ConnectResponse {
            IsSucceed = false
        };
    }

    private async Task<bool> VerifyCredentials(Credentials requestCredentials) {
        return await _userRepository.CredsExistsAsync(requestCredentials.Name, requestCredentials.Password);
    }

    private async Task<bool> ValidateRefreshToken(RefreshTokenRequest request) {
        var storedToken = await _authenticationRepository.FindToken(request.Name, request.Device);
        Console.WriteLine($"Stored token: {storedToken}");
        return storedToken!=null && storedToken == request.RefreshToken;
    }
}
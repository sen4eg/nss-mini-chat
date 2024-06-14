using MiniServer.Data.Repository;
using MiniServer.Utils;

namespace MiniServer.Services; 

public interface IAuthenticationService
{
    string GenerateToken(string name);
    string GenerateRefreshToken(string username, Device device);
    Task<ConnectResponse> RefreshTokenAsync(RefreshTokenRequest request);
    // Add other methods as needed
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IValidationTokenRepository _validationTokenRepository;

    public AuthenticationService(IValidationTokenRepository validationTokenRepository)
    {
        _validationTokenRepository = validationTokenRepository;
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
        _validationTokenRepository.StoreToken(username, device, refresh);
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
            RefreshToken = GenerateRefreshToken(request.Name, request.Device)
        };
    }

    private async Task<bool> ValidateRefreshToken(RefreshTokenRequest request) {
        return await _validationTokenRepository.TokenExists(request);
    }
}
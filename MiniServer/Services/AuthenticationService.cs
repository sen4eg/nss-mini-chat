using System.Security.Claims;
using MiniProtoImpl;
using MiniServer.Data.Repository;
using MiniServer.Utils;

namespace MiniServer.Services; 

public interface IAuthenticationService
{
    string GenerateToken(long id);
    string GenerateRefreshToken(string username, Device device);
    Task<ConnectResponse> RefreshTokenAsync(RefreshTokenRequest request);
    // Add other methods as needed
    Task<ConnectResponse> Authenticate(ConnectRequest request);
    TokenIdentity GetIdentity(string token);
}

public class TokenIdentity {
    public readonly ClaimsPrincipal Claims;

    public TokenIdentity(ClaimsPrincipal identity) {
        this.Claims = identity;
    }

    public string? GetUserId() {
        return TokenHelper.GetUserIdFromClaimsPrincipal(Claims);
    }

    public bool IsAuthenticated() {
        return Claims.Identity?.IsAuthenticated ?? false;
    }
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
    public string GenerateToken(long uid)
    {
        return TokenHelper.GenerateAccessToken(uid.ToString());
    }
    
    public string GenerateRefreshToken(string username, Device device)
    {
        var refresh = TokenHelper.GenerateRefreshToken();
        _authenticationRepository.StoreToken(username, device, refresh);
        return refresh;
    }

    public async Task<ConnectResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var foundToken = await _authenticationRepository.FindAuthRefreshToken(request.Name, request.Device);
        if (foundToken == null)
        {
            return new ConnectResponse
            {
                IsSucceed = false,
            };
        }

        return new ConnectResponse
        {
            IsSucceed = true,
            Token = GenerateToken(foundToken.User.UserId),
            RefreshToken = request.RefreshToken
        };
    }

    public async Task<ConnectResponse> Authenticate(ConnectRequest request) {
        var token = await _authenticationRepository.FindToken(request.Credentials.Name, request.Device);
        var uid = await GetUidWithCredentials(request.Credentials);
        if (uid == null)
            return new ConnectResponse {
                IsSucceed = false
            };
        long userId = uid.Value;
        if (!string.IsNullOrEmpty(token)) {
            return new ConnectResponse {
                IsSucceed = true,
                Token = GenerateToken(userId),
                RefreshToken = token // Reuse the existing refresh token on same device
            };
        }
        return new ConnectResponse {
            IsSucceed = true,
            Token = GenerateToken(userId),
            RefreshToken = GenerateRefreshToken(request.Credentials.Name, request.Device)
        };
    }

    public TokenIdentity GetIdentity(string token) {
        TokenHelper.DecipherToken(token, out var Identity);
        return new TokenIdentity(Identity);
    }

    private async Task<long?> GetUidWithCredentials(Credentials requestCredentials) {
        return await _userRepository.FindWithCredentials(requestCredentials.Name, requestCredentials.Password);
    }
}
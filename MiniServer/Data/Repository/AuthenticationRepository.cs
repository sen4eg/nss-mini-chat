using Microsoft.EntityFrameworkCore;
using MiniServer.Data.Model;
using MiniProtoImpl;
namespace MiniServer.Data.Repository;

public interface IValidationTokenRepository {
    Task<bool> TokenExists(string username, string device, string token);
    Task StoreToken(string username, Device device, string token);
    Task DeleteToken(string username, string device);
    Task<bool> TokenExists(RefreshTokenRequest requestRefreshToken);
    Task<string> FindToken(string credentialsName, Device requestDevice);
    
    Task<AuthenicatedToken?> FindAuthRefreshToken(string name, Device device);
}

public class ValidationTokenRepository : IValidationTokenRepository {
    private readonly ChatContext _context;

    public ValidationTokenRepository(ChatContext context) {
        _context = context;
    }

    public async Task<bool> TokenExists(string username, string device, string token) {
        return await _context.ValidationTokens.AnyAsync(t => t.Username == username && t.Device == device && t.Token == token);
    }

    public async Task StoreToken(string username, Device device, string token) {
        var validationToken = new AuthenicatedToken {
            Username = username,
            Device = device.Name,
            ip = device.Ip,
            OS = device.Os,
            Token = token
        };

        _context.ValidationTokens.Add(validationToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteToken(string username, string device) {
        var token = await _context.ValidationTokens.FirstOrDefaultAsync(t => t.Username == username && t.Device == device);
        if (token != null) {
            _context.ValidationTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> TokenExists(RefreshTokenRequest requestRefreshToken) {
        return await TokenExists(
            requestRefreshToken.Name,
            requestRefreshToken.Device.Name,
            requestRefreshToken.RefreshToken
        );
    }
    
    public async Task<string> FindToken(string credentialsName, Device requestDevice)
    {
        // Add null checks for input parameters
        if (string.IsNullOrEmpty(credentialsName))
        {
            throw new ArgumentNullException(nameof(credentialsName));
        }

        if (requestDevice == null || string.IsNullOrEmpty(requestDevice.Name))
        {
            throw new ArgumentNullException(nameof(requestDevice));
        }

        try
        {
            var tokenEntity = await _context.ValidationTokens
                .FirstOrDefaultAsync(t => t.Username == credentialsName && t.Device == requestDevice.Name);

            // Return the token if found, otherwise return an empty string
            return tokenEntity?.Token ?? string.Empty; // or you can return a default value like "N/A" if desired
        }
        catch (Exception ex)
        {
            // throw;
            
        }

        return "";
    }

    public Task<AuthenicatedToken?> FindAuthRefreshToken(string name, Device device) {
        return _context.ValidationTokens.Include(o => o.User).FirstOrDefaultAsync(t => t.Username == name && t.Device == device.Name);
    }
}
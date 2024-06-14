using Microsoft.EntityFrameworkCore;
using MiniServer.Data.Model;

namespace MiniServer.Data.Repository;

public interface IValidationTokenRepository {
    Task<bool> TokenExists(string username, string device, string token);
    Task StoreToken(string username, Device device, string token);
    Task DeleteToken(string username, string device);
    Task<bool> TokenExists(RefreshTokenRequest requestRefreshToken);
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
            ip = device.Name,
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
}
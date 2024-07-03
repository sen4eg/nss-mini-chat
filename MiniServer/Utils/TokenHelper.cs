using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiniServer.Services;

namespace MiniServer.Utils; 

public static class TokenHelper
{
    // Constants for JWT configuration
    // secret must be atleast 256 bits long
    private const string Secret = "VerySecretKeyVerySefsfsfsfsfsfsfsfsfsfsfsfscure"; // Change this to a more secure key in production
    private const string Issuer = "MiniServer(VeryTrustworthy)";
    private const string Audience = "MiniChatClient";

    public static string GenerateAccessToken(string userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }),
            Expires = DateTime.UtcNow.AddMinutes(15), // Access token expires in 15 minutes
            Issuer = Issuer,
            Audience = Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public static bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedSecurityToken);
            return true; // Token is valid
        }
        catch (Exception ex)
        {
            // Log or handle exception as needed
            return false; // Token validation failed
        }
    }
    public static string? GetUserIdFromClaimsPrincipal(ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim;
    }
    public static void DecipherToken(string token, out ClaimsPrincipal o) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(Secret);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = Issuer,
            ValidAudience = Audience,
            ClockSkew = TimeSpan.Zero
        };
    
        try {
            var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            o = claimsPrincipal;
            return;
        }
        catch (Exception e) {
            o = new ClaimsPrincipal();
            return;
        }
    }
}
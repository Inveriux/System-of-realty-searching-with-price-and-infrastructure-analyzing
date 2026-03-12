using UserService.Services.JwtProviders.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace UserService.Services.JwtProviders;

public class RefreshTokenProvider : IRefreshTokenProvider
{
    public string GenerateRefreshToken()
    {
        var randomByres = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomByres);
    }

    public string HashToken(string Token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(Token));
        return Convert.ToBase64String(hash);
    }
}
using UserService.Services.JwtProviders.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace UserService.Services.JwtProviders;

public sealed class DomainJwtProvider(string issuer, string audience, string secret)
    : IDomainJwtProvider
{
    private const int ExpirationMinutes = 30;

    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(secret));

    public string GenerateToken(Guid userId, IEnumerable<Claim>? extraClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
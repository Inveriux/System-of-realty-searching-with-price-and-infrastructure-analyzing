using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.IntegrationTests.Common;

internal static class TestJwtTokenFactory
{
    public const string Authority = "https://tests.diploma.local";
    public const string Audience = "backend-tests";
    public const string Key = "integration-tests-signing-key-1234567890";

    public static string CreateToken(
        Guid userId,
        string email,
        string name,
        string role,
        DateTime? expiresUtc = null)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Authority,
            audience: Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            ],
            expires: expiresUtc ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

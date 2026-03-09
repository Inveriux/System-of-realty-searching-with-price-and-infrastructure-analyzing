using System.Security.Claims;

namespace UserService.Services.JwtProviders.Abstractions;

public interface IDomainJwtProvider
{
    string GenerateToken(Guid userId, IEnumerable<Claim>? extraClaims = null);
}
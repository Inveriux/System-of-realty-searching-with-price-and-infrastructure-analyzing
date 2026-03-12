using BasicBuildingBlocks.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BasicBuildingBlocks.Services;

public class CurrentUserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
{
    public string GetRole() => GetRequiredClaim(ClaimTypes.Role);

    public Guid GetUserId()
    {
        var value = GetRequiredClaim(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("User id claim is invalid.");

        return userId;
    }

    public string GetUserName() => GetRequiredClaim(ClaimTypes.Name);

    public string GetEmail() => GetRequiredClaim(ClaimTypes.Email);

    public string GetGoogleId() => GetRequiredClaim(ClaimTypes.NameIdentifier);

    public string? GetImageUrl() =>
        httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "image_url")?.Value;

    private string GetRequiredClaim(string claimType)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException("User is not authenticated.");

        return httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value
            ?? throw new UnauthorizedAccessException($"Required claim '{claimType}' was not found.");
    }
}

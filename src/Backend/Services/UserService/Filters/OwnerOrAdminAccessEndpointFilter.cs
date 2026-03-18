using BasicBuildingBlocks.Authorization;
using BasicBuildingBlocks.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using UserService.Models.Enums;

namespace UserService.Filters;

public class OwnerOrAdminAccessEndpointFilter(ICurrentUserProvider currentUserProvider) 
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        var authorizationService = context.HttpContext
                .RequestServices
                .GetRequiredService<IAuthorizationService>();
            
        var resourceEmail = context.HttpContext.GetRouteValue("email") as string
            ?? currentUserProvider.GetEmail();

        var result = await authorizationService.AuthorizeAsync(
            context.HttpContext.User,
            resourceEmail,
            new OwnerOrAdminAccessAuthorizationRequirement(UserRoles.Admin));

        if (!result.Succeeded)
            return Results.Forbid();

        return await next(context);
    }
}

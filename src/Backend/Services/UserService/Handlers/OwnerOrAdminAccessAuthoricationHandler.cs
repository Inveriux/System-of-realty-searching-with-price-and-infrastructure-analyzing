using BasicBuildingBlocks.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace UserService.Handlers;

public class OwnerOrAdminAccessAuthorizationRequirement(string adminRoleName) : IAuthorizationRequirement
{
    public string AdminRoleName { get; } = adminRoleName;
}

public class OwnerOrAdminAccessAuthorizationHandler(ICurrentUserProvider currentUserProvider)
    : AuthorizationHandler<OwnerOrAdminAccessAuthorizationRequirement, string>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, 
        OwnerOrAdminAccessAuthorizationRequirement requirement,
        string resourceEmail)
    {
        if (context.User.IsInRole(requirement.AdminRoleName))
        {
            context.Succeed(requirement);
            return;
        }

        var userEmail = currentUserProvider.GetEmail();
        if(userEmail is null)
            return;

        if(resourceEmail == userEmail)
            context.Succeed(requirement);

        return;
    }
}

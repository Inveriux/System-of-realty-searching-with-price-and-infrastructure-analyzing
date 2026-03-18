using Microsoft.AspNetCore.Authorization;

namespace BasicBuildingBlocks.Authorization;

public class OwnerOrAdminAccessAuthorizationRequirement(string adminRoleName) : IAuthorizationRequirement
{
    public string AdminRoleName { get; } = adminRoleName;
}

public class OwnerOrAdminAccessAuthorizationHandler<TResource>(IOwnerResourceAccessor<TResource> ownerResourceAccessor)
    : AuthorizationHandler<OwnerOrAdminAccessAuthorizationRequirement, TResource>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        OwnerOrAdminAccessAuthorizationRequirement requirement,
        TResource resource)
    {
        if (context.User.IsInRole(requirement.AdminRoleName))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (ownerResourceAccessor.IsOwner(resource))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

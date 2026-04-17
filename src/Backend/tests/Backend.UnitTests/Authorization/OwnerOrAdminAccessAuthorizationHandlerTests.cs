using System.Security.Claims;
using BasicBuildingBlocks.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Backend.UnitTests.Authorization;

[Trait("Category", "Unit")]
public sealed class OwnerOrAdminAccessAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsAdmin_ShouldSucceedEvenIfResourceIsNotOwned()
    {
        var accessor = new Mock<IOwnerResourceAccessor<Guid>>();
        accessor.Setup(x => x.IsOwner(It.IsAny<Guid>())).Returns(false);

        var handler = new OwnerOrAdminAccessAuthorizationHandler<Guid>(accessor.Object);
        var context = new AuthorizationHandlerContext(
            [new OwnerOrAdminAccessAuthorizationRequirement("Admin")],
            CreatePrincipal(role: "Admin"),
            resource: Guid.NewGuid());

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsNeitherAdminNorOwner_ShouldNotSucceed()
    {
        var accessor = new Mock<IOwnerResourceAccessor<Guid>>();
        accessor.Setup(x => x.IsOwner(It.IsAny<Guid>())).Returns(false);

        var handler = new OwnerOrAdminAccessAuthorizationHandler<Guid>(accessor.Object);
        var context = new AuthorizationHandlerContext(
            [new OwnerOrAdminAccessAuthorizationRequirement("Admin")],
            CreatePrincipal(role: "User"),
            resource: Guid.NewGuid());

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    private static ClaimsPrincipal CreatePrincipal(string role)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, role)],
            authenticationType: "test"));
}

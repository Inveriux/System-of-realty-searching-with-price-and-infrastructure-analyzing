using BasicBuildingBlocks.Services.Abstractions;
using FluentAssertions;
using Moq;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;
using UserService.Models;
using UserService.UseCases.Users;
using Xunit;

namespace Backend.UnitTests.Users;

[Trait("Category", "Unit")]
public sealed class UpdateOwnProfileCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenProfileContainsWhitespace_ShouldTrimOptionalFields()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("Old Name", "user@example.com", null);
        user.Id = userId;
        user.PhoneNumber = "+375291112233";

        var repository = new Mock<IUserRepository>();
        repository.Setup(x => x.GetByIdAsync(userId, true)).ReturnsAsync(user);
        repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        repository.Setup(x => x.GetUserRoleAsync(user)).ReturnsAsync("User");

        var currentUserProvider = new Mock<ICurrentUserProvider>();
        currentUserProvider.Setup(x => x.GetUserId()).Returns(userId);

        var mapper = new Mock<AutoMapper.IMapper>();
        mapper.Setup(x => x.Map<GetUserDto>(user)).Returns(() => new GetUserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email!,
            AccountImageUrl = user.AccountImageUrl,
            PhoneNumber = user.PhoneNumber,
            EmailConfirmed = user.EmailConfirmed,
            HasPassword = !string.IsNullOrWhiteSpace(user.PasswordHash),
            IsGoogleAccount = user.GoogleAuthentication is not null,
            Role = string.Empty
        });

        var handler = new UpdateOwnProfileCommandHandler(repository.Object, currentUserProvider.Object, mapper.Object);

        var result = await handler.Handle(
            new UpdateOwnProfileCommand(new UpdateOwnProfileDto(
                "  New Name  ",
                "   ",
                "  https://cdn.example.com/avatar.png  ")),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        user.Name.Should().Be("New Name");
        user.PhoneNumber.Should().BeNull();
        user.AccountImageUrl.Should().Be("https://cdn.example.com/avatar.png");
        result.Data!.Role.Should().Be("User");
    }
}

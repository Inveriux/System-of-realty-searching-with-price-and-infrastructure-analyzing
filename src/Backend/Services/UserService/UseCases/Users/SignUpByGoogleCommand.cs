using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.Services.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using Microsoft.AspNetCore.Identity;
using UserService.Models.Enums;
using UserService.Models;
using MediatR;

namespace UserService.UseCases.Users;

public record SignUpByGoogleCommand() : IRequest<Result<bool>>;

public class SignUpByGoogleHandler(IUserRepository userRepository,
    UserManager<User> userManager,
    ICurrentUserProvider currentUserProvider)
    : IRequestHandler<SignUpByGoogleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SignUpByGoogleCommand request, 
        CancellationToken cancellationToken = default)
    {
        var existingUser = await userManager.FindByEmailAsync(currentUserProvider.GetEmail());
        if (existingUser is not null)
            return Result.Fail<bool>("Email is already in use.")
                .WithError($"User with {currentUserProvider.GetEmail()} email already exists.", propertyName: nameof(User));

        var user = User.Create(currentUserProvider.GetUserName(), 
            currentUserProvider.GetEmail(), 
            currentUserProvider.GetImageUrl());
        user.EmailConfirmed = true;
        
        user.GoogleAuthentication = new GoogleAuthentication
        {
            Id = Guid.NewGuid(),
            GoogleId = currentUserProvider.GetGoogleId()
        };

        await userRepository.CreateGoogleUserAsync(user, UserRoles.User); 
        await userRepository.SaveChangesAsync();
        return Result.Ok(true);
    }
}

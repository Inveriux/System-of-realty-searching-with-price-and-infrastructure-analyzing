using UserService.Services.JwtProviders.Abstractions;
using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.Service.Abstractions;
using BasicBuildingBlocks.Results;
using System.Security.Claims;
using UserService.DTOs;
using MediatR;

namespace UserService.UseCases.Users;

public record SignInByGoogleCommand : IRequest<Result<SignInUserResponseDto>>;

public class SignInByGoogleHandler(ICurrentUserProvider currentUserProvider,
    IUserRepository userRepository,
    IDomainJwtProvider domainJwtProvider) 
    : IRequestHandler<SignInByGoogleCommand, Result<SignInUserResponseDto>>
{
    public async Task<Result<SignInUserResponseDto>> Handle(SignInByGoogleCommand request, 
        CancellationToken cancellationToken = default)
    {
        var email = currentUserProvider.GetEmail();
        var user = await userRepository.GetByEmailAsync(email, false);
        if (user is null)
            return Result.Fail<SignInUserResponseDto>("Invalid email.")
                .WithError($"User with {email} email not found."); 

        var userRole = await userRepository.GetUserRoleAsync(user)
            ?? throw new Exception("User role not found.");

        var name = user.Name;
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("User name cannot be empty.");

        var token = domainJwtProvider.GenerateToken(user.Id,
        [
            new Claim(ClaimTypes.Role, userRole),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name)
        ]);

        return Result.Ok(new SignInUserResponseDto(true, token));
    }
}

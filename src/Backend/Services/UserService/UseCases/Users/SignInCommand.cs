using UserService.Services.JwtProviders.Abstractions;
using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.Results;
using System.Security.Claims;
using UserService.DTOs;
using MediatR;

namespace UserService.UseCases.Users;

public record SignInCommand(SignInUserDto SignInUserDto) : IRequest<Result<SignInUserResponseDto>>;

public class SignInHandler(IUserRepository userRepository,
    IDomainJwtProvider domainJwtProvider) 
    : IRequestHandler<SignInCommand, Result<SignInUserResponseDto>>
{
    public async Task<Result<SignInUserResponseDto>> Handle(SignInCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.SignInUserDto;
        var user = await userRepository.GetByEmailAsync(dto.Email, false);
        if (user is null)
            return Result.Fail<SignInUserResponseDto>("Invalid email.")
                .WithError($"User with {dto.Email} email not found."); 

        var email = dto.Email;
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

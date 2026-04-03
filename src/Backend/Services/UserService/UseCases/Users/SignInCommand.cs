using UserService.Services.JwtProviders.Abstractions;
using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using UserService.Models;
using UserService.DTOs;
using MediatR;

namespace UserService.UseCases.Users;

public record SignInCommand(SignInUserDto SignInUserDto) : IRequest<Result<SignInUserResponseDto>>;

public class SignInHandler(IUserRepository userRepository,
    UserManager<User> userManager,
    IDomainJwtProvider domainJwtProvider,
    IRefreshTokenProvider refreshTokenProvider) 
    : IRequestHandler<SignInCommand, Result<SignInUserResponseDto>>
{
    public async Task<Result<SignInUserResponseDto>> Handle(SignInCommand request, CancellationToken cancellationToken = default)
    {
        var dto = request.SignInUserDto;
        var user = await userRepository.GetByEmailAsync(dto.Email, true);
        if (user is null)
            return Result.Fail<SignInUserResponseDto>("Некорректный email.")
                .WithError("Пользователь с таким email не найден.", propertyName: nameof(User)); 

        if (!user.EmailConfirmed)
            return Result.Fail<SignInUserResponseDto>("Электронная почта не подтверждена.")
                .WithError("Подтвердите email перед входом.", propertyName: nameof(User.EmailConfirmed));

        var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return Result.Fail<SignInUserResponseDto>("Некорректный пароль.")
                .WithError("Неверный пароль.", propertyName: nameof(User));

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

        var refreshToken = refreshTokenProvider.GenerateRefreshToken();
        var hash = refreshTokenProvider.HashToken(refreshToken);
        if (user.RefreshToken is null)
        {
            user.RefreshToken = new()
            {
                Id = user.Id,
                TokenHash = hash,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
            };
        }
        else
        {
            user.RefreshToken.TokenHash = hash;
            user.RefreshToken.ExpiresAt = DateTimeOffset.UtcNow.AddDays(7);
        }

        await userRepository.SaveChangesAsync();

        return Result.Ok(new SignInUserResponseDto(true, token, refreshToken));
    }
}

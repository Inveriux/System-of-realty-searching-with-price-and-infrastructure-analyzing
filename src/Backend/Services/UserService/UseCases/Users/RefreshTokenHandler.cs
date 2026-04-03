using UserService.Services.JwtProviders.Abstractions;
using UserService.Data.Repositiroes.Abstractions;
using BasicBuildingBlocks.BusinessResults;
using System.Security.Claims;
using UserService.Models;
using UserService.DTOs;
using MediatR;

namespace UserService.UseCases.Users;

public record RefreshTokenCommnand(RefreshTokenRequestDto Dto) : IRequest<Result<RefreshTokenResponseDto>>;

public class RefreshTokenCommandHandler(IUserRepository userRepository,
    IDomainJwtProvider domainJwtProvider,
    IRefreshTokenProvider refreshTokenProvider)
    : IRequestHandler<RefreshTokenCommnand, Result<RefreshTokenResponseDto>>
{
    private const int RefreshTokenValidityDays = 7;


    public async Task<Result<RefreshTokenResponseDto>> Handle(RefreshTokenCommnand request, 
        CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenProvider.HashToken(request.Dto.RefreshToken);
        var user = await userRepository.GetByRefreshTokenAsync(tokenHash);

        if (user is null)
            return Result.Fail<RefreshTokenResponseDto>("Некорректный refresh token.")
                .WithError("Пользователь с таким refresh token не найден.", propertyName: nameof(RefreshToken));

        if (user.RefreshToken.TokenHash != tokenHash || user.RefreshToken.ExpiresAt < DateTimeOffset.UtcNow)
            return Result.Fail<RefreshTokenResponseDto>("Некорректный или просроченный refresh token.")
                .WithError("Срок действия refresh token истек. Войдите снова.", propertyName: nameof(RefreshToken));

        var newRefreshToken = refreshTokenProvider.GenerateRefreshToken();
        user.RefreshToken.TokenHash = refreshTokenProvider.HashToken(newRefreshToken);
        user.RefreshToken.ExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenValidityDays);

        if(user.Email is null)
            throw new Exception("User email cannot be null.");

        var newAccessToken = domainJwtProvider.GenerateToken(user.Id, 
            [
                new Claim(ClaimTypes.Role, await userRepository.GetUserRoleAsync(user) ?? throw new Exception("User role not found.")),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            ]);

        await userRepository.SaveChangesAsync();

        return Result.Ok(new RefreshTokenResponseDto(newRefreshToken, newAccessToken));
    }
}

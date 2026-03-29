using BasicBuildingBlocks.BusinessResults;
using MediatR;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;
using UserService.Models;
using UserService.Services.EmailVerification.Abstractions;

namespace UserService.UseCases.Users;

public sealed record VerifyEmailCodeCommand(VerifyEmailCodeDto Dto) : IRequest<Result<bool>>;

public sealed class VerifyEmailCodeHandler(
    IUserRepository userRepository,
    IEmailVerificationService emailVerificationService)
    : IRequestHandler<VerifyEmailCodeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(VerifyEmailCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Dto.Email, true);
        if (user is null)
            return Result.Fail<bool>("Invalid email.")
                .WithError("User not found.", propertyName: nameof(User.Email));

        if (user.EmailConfirmed)
            return Result.Ok(true);

        var isCodeValid = await emailVerificationService.VerifyCodeAsync(user, request.Dto.Code, cancellationToken);
        if (!isCodeValid)
            return Result.Fail<bool>("Invalid verification code.")
                .WithError("Verification code is invalid or expired.", propertyName: nameof(VerifyEmailCodeDto.Code));

        user.EmailConfirmed = true;
        await userRepository.SaveChangesAsync();
        return Result.Ok(true);
    }
}

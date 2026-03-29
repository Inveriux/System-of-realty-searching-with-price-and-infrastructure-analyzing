using BasicBuildingBlocks.BusinessResults;
using MediatR;
using UserService.Data.Repositiroes.Abstractions;
using UserService.DTOs;
using UserService.Models;
using UserService.Services.EmailVerification.Abstractions;

namespace UserService.UseCases.Users;

public sealed record ResendEmailVerificationCodeCommand(ResendEmailVerificationCodeDto Dto) : IRequest<Result<bool>>;

public sealed class ResendEmailVerificationCodeHandler(
    IUserRepository userRepository,
    IEmailVerificationService emailVerificationService)
    : IRequestHandler<ResendEmailVerificationCodeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ResendEmailVerificationCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Dto.Email, true);
        if (user is null)
            return Result.Fail<bool>("Invalid email.")
                .WithError("User not found.", propertyName: nameof(User.Email));

        if (user.EmailConfirmed)
            return Result.Fail<bool>("Email is already verified.")
                .WithError("Email is already verified.", propertyName: nameof(User.EmailConfirmed));

        if (!emailVerificationService.IsConfigured)
            return Result.Fail<bool>("Email verification is not configured.")
                .WithError("SMTP sender is not configured.", propertyName: nameof(ResendEmailVerificationCodeDto.Email));

        await emailVerificationService.SendCodeAsync(user, cancellationToken);
        return Result.Ok(true);
    }
}

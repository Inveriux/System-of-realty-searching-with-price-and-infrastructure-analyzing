using UserService.Models;

namespace UserService.Services.EmailVerification.Abstractions;

public interface IEmailVerificationService
{
    bool IsConfigured { get; }
    Task SendCodeAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> VerifyCodeAsync(User user, string code, CancellationToken cancellationToken = default);
}

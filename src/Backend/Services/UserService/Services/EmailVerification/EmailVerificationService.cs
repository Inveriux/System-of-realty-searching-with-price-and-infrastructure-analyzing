using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Models;
using UserService.Services.Email.Abstractions;
using UserService.Services.EmailVerification.Abstractions;

namespace UserService.Services.EmailVerification;

public sealed class EmailVerificationService(
    ApplicationDbContext context,
    IEmailSender emailSender,
    IOptions<EmailVerificationOptions> options)
    : IEmailVerificationService
{
    private readonly EmailVerificationOptions verificationOptions = options.Value;

    public bool IsConfigured => emailSender.IsConfigured;

    public async Task SendCodeAsync(User user, CancellationToken cancellationToken = default)
    {
        if (!emailSender.IsConfigured)
            throw new InvalidOperationException("Email verification sender is not configured.");

        var now = DateTimeOffset.UtcNow;
        var activeCodes = await context.EmailVerificationCodes
            .Where(x => x.Email == user.Email && !x.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var existingCode in activeCodes)
        {
            existingCode.IsUsed = true;
        }

        var code = GenerateCode();

        context.EmailVerificationCodes.Add(new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Email = user.Email!,
            CodeHash = ComputeHash(code),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(Math.Max(1, verificationOptions.CodeLifetimeMinutes)),
            IsUsed = false
        });

        await context.SaveChangesAsync(cancellationToken);

        var subject = "Подтверждение почты в Domio";
        var htmlBody = $"""
            <div style="margin:0;background:#f4f1fb;padding:32px 16px;font-family:Arial,'Segoe UI',sans-serif;color:#1f1630;">
              <div style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 18px 48px rgba(72,35,117,0.12);">
                <div style="padding:28px 32px 18px;background:linear-gradient(135deg,#1f1238 0%,#5b2fb0 100%);color:#ffffff;">
                  <div style="display:inline-block;padding:8px 14px;border-radius:999px;background:rgba(255,255,255,0.14);font-size:13px;font-weight:700;letter-spacing:0.08em;text-transform:uppercase;">Domio</div>
                  <h1 style="margin:18px 0 8px;font-size:28px;line-height:1.2;">Подтвердите вашу почту</h1>
                  <p style="margin:0;font-size:15px;line-height:1.6;color:rgba(255,255,255,0.82);">
                    Используйте этот код, чтобы завершить регистрацию и войти в приложение.
                  </p>
                </div>
                <div style="padding:28px 32px 32px;">
                  <div style="margin:0 0 14px;font-size:14px;line-height:1.6;color:#5b5470;">
                    Код подтверждения:
                  </div>
                  <div style="margin:0 0 22px;padding:18px 20px;border-radius:18px;background:#f6f0ff;border:1px solid #e8dafd;font-size:32px;font-weight:700;letter-spacing:0.32em;text-align:center;color:#5b2fb0;">
                    {code}
                  </div>
                  <p style="margin:0 0 12px;font-size:14px;line-height:1.7;color:#5b5470;">
                    Код действует {Math.Max(1, verificationOptions.CodeLifetimeMinutes)} минут.
                  </p>
                  <p style="margin:0;font-size:14px;line-height:1.7;color:#5b5470;">
                    Если вы не запрашивали регистрацию в Domio, просто проигнорируйте это письмо.
                  </p>
                </div>
              </div>
            </div>
            """;

        await emailSender.SendAsync(user.Email!, subject, htmlBody, cancellationToken);
    }

    public async Task<bool> VerifyCodeAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var hashedCode = ComputeHash(code);

        var verificationCode = await context.EmailVerificationCodes
            .Where(x => x.UserId == user.Id && x.Email == user.Email)
            .Where(x => !x.IsUsed && x.ExpiresAtUtc >= now)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (verificationCode is null || !string.Equals(verificationCode.CodeHash, hashedCode, StringComparison.Ordinal))
            return false;

        verificationCode.IsUsed = true;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private string GenerateCode()
    {
        var codeLength = Math.Clamp(verificationOptions.CodeLength, 4, 8);
        var maxValue = (int)Math.Pow(10, codeLength);
        return RandomNumberGenerator.GetInt32(0, maxValue).ToString($"D{codeLength}");
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value.Trim()));
        return Convert.ToHexString(bytes);
    }
}

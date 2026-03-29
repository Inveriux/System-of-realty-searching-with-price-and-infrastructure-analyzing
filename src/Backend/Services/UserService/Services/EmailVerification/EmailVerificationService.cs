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

        var subject = "Код подтверждения email";
        var htmlBody = $"""
            <div style="font-family:Arial,sans-serif;line-height:1.5">
              <h2 style="margin-bottom:12px">Подтверждение почты</h2>
              <p>Ваш код подтверждения для регистрации в Diploma:</p>
              <div style="font-size:28px;font-weight:700;letter-spacing:6px;margin:16px 0">{code}</div>
              <p>Код действует {Math.Max(1, verificationOptions.CodeLifetimeMinutes)} минут.</p>
              <p>Если вы не запрашивали регистрацию, просто проигнорируйте это письмо.</p>
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

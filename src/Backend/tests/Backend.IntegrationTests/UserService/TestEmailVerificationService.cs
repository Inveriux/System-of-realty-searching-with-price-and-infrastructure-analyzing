using UserService.Models;
using UserService.Services.EmailVerification.Abstractions;

namespace Backend.IntegrationTests.UserServiceTests;

internal sealed class TestEmailVerificationService : IEmailVerificationService
{
    private readonly Dictionary<string, string> _codes = new(StringComparer.OrdinalIgnoreCase);

    public bool IsConfigured => true;

    public Task SendCodeAsync(User user, CancellationToken cancellationToken = default)
    {
        _codes[user.Email!] = "123456";
        return Task.CompletedTask;
    }

    public Task<bool> VerifyCodeAsync(User user, string code, CancellationToken cancellationToken = default)
    {
        var isValid = _codes.TryGetValue(user.Email!, out var storedCode)
            && string.Equals(storedCode, code, StringComparison.Ordinal);

        if (isValid)
        {
            _codes.Remove(user.Email!);
        }

        return Task.FromResult(isValid);
    }

    public string GetCode(string email) => _codes[email];
}

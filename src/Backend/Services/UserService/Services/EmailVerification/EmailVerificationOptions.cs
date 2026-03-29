namespace UserService.Services.EmailVerification;

public sealed class EmailVerificationOptions
{
    public static readonly string SectionName = "EmailVerification";

    public int CodeLength { get; init; } = 6;
    public int CodeLifetimeMinutes { get; init; } = 10;
}

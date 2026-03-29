namespace UserService.Services.Email;

public sealed class SmtpEmailOptions
{
    public static readonly string SectionName = "Email:Smtp";

    public string? Host { get; init; }
    public int Port { get; init; } = 587;
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string? SenderEmail { get; init; }
    public string? SenderName { get; init; }
    public bool EnableSsl { get; init; } = true;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(UserName) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(SenderEmail);
}

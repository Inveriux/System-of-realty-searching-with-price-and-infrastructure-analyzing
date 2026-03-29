namespace UserService.Models;

public sealed class EmailVerificationCode
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string CodeHash { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public bool IsUsed { get; set; }
}

namespace UserService.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public User User { get; set; } = null!;
}
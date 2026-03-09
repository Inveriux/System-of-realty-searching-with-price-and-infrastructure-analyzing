namespace UserService.Models;

public class GoogleAuthentication
{
    public Guid Id { get; set; } 
    public User User { get; set; } = null!;
    public string GoogleId { get; set; } = null!;
}
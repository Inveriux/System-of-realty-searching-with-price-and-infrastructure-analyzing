using Microsoft.AspNetCore.Identity;

namespace UserService.Models;

public class User : IdentityUser<Guid>
{
    public static User Create(string name, string email, string? accountImageUrl = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserName = email,
            Email = email,
            AccountImageUrl = accountImageUrl
        };
    }

    public string Name { get; set; } = null!;
    public string? AccountImageUrl { get; set; }
    public GoogleAuthentication? GoogleAuthentication { get; set; }
    public RefreshToken RefreshToken { get; set; } = null!;
}

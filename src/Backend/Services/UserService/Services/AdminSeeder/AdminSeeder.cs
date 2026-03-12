using UserService.Services.AdminSeeder.Abstractions;
using Microsoft.AspNetCore.Identity;
using UserService.Models.Enums;
using UserService.Models;

namespace UserService.Services.AdminSeeder;

public class AdminSeeder(UserManager<User> userManager, 
    IPasswordHasher<User> passwordHasher,
    ApplicationDbContext context) : IAdminSeeder
{
    public async Task SeedAdminAsync(string name, string email, string password, string? accountImageUrl = null)
    {
        var user = User.Create(name, email, accountImageUrl);
        await userManager.CreateAsync(user);
        var result = await userManager.AddPasswordAsync(user, password);
        if(!result.Succeeded)
            throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, UserRoles.Admin);
    }

    public void SeedAdmin(string name, string email, string password, string? accountImageUrl = null)
    {
        var user = User.Create(name, email, accountImageUrl);
        var hash = passwordHasher.HashPassword(user, password);
        user.PasswordHash = hash;
        context.Add(user);
        context.UserRoles.Add(new IdentityUserRole<Guid>
        {
            RoleId = context.Roles.First(r => r.Name == UserRoles.Admin).Id,
            UserId = user.Id
        });
    }

}
using UserService.Services.AdminSeeder.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Models.Enums;
using UserService.Models;

namespace UserService.Services.AdminSeeder;

public class AdminSeeder(UserManager<User> userManager, 
    IPasswordHasher<User> passwordHasher,
    ApplicationDbContext context) : IAdminSeeder
{
    public async Task SeedAdminAsync(
        string name,
        string email,
        string password,
        string? accountImageUrl = null,
        string? phoneNumber = null)
    {
        var admin = await FindExistingAdminAsync();
        if (admin is null)
        {
            admin = User.Create(name, email, accountImageUrl);
            admin.EmailConfirmed = true;
            admin.PhoneNumber = phoneNumber;

            var createResult = await userManager.CreateAsync(admin);
            if (!createResult.Succeeded)
                throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));

            var addPasswordResult = await userManager.AddPasswordAsync(admin, password);
            if(!addPasswordResult.Succeeded)
                throw new Exception(string.Join("; ", addPasswordResult.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(admin, UserRoles.Admin);
            return;
        }

        admin.Name = name;
        admin.Email = email;
        admin.UserName = email;
        admin.NormalizedEmail = email.ToUpperInvariant();
        admin.NormalizedUserName = email.ToUpperInvariant();
        admin.AccountImageUrl = accountImageUrl;
        admin.EmailConfirmed = true;
        admin.PhoneNumber = phoneNumber;
        admin.PasswordHash = userManager.PasswordHasher.HashPassword(admin, password);

        var updateResult = await userManager.UpdateAsync(admin);
        if (!updateResult.Succeeded)
            throw new Exception(string.Join("; ", updateResult.Errors.Select(e => e.Description)));
    }

    public void SeedAdmin(
        string name,
        string email,
        string password,
        string? accountImageUrl = null,
        string? phoneNumber = null)
    {
        var admin = FindExistingAdmin();
        if (admin is null)
        {
            admin = User.Create(name, email, accountImageUrl);
            context.Add(admin);
            context.UserRoles.Add(new IdentityUserRole<Guid>
            {
                RoleId = context.Roles.First(r => r.Name == UserRoles.Admin).Id,
                UserId = admin.Id
            });
        }

        admin.Name = name;
        admin.Email = email;
        admin.UserName = email;
        admin.NormalizedEmail = email.ToUpperInvariant();
        admin.NormalizedUserName = email.ToUpperInvariant();
        admin.AccountImageUrl = accountImageUrl;
        admin.EmailConfirmed = true;
        admin.PhoneNumber = phoneNumber;
        admin.PasswordHash = passwordHasher.HashPassword(admin, password);
    }

    private User? FindExistingAdmin()
    {
        var adminRoleId = context.Roles.FirstOrDefault(r => r.Name == UserRoles.Admin)?.Id;
        if (adminRoleId is null)
            return null;

        var adminUserId = context.UserRoles
            .Where(x => x.RoleId == adminRoleId)
            .Select(x => x.UserId)
            .FirstOrDefault();

        return adminUserId == Guid.Empty
            ? null
            : context.Users.FirstOrDefault(x => x.Id == adminUserId);
    }

    private async Task<User?> FindExistingAdminAsync()
    {
        var adminRoleId = await context.Roles
            .Where(r => r.Name == UserRoles.Admin)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync();

        if (adminRoleId is null)
            return null;

        var adminUserId = await context.UserRoles
            .Where(x => x.RoleId == adminRoleId)
            .Select(x => (Guid?)x.UserId)
            .FirstOrDefaultAsync();

        return adminUserId is null
            ? null
            : await context.Users.FirstOrDefaultAsync(x => x.Id == adminUserId.Value);
    }
}

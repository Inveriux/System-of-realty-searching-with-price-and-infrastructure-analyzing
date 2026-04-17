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
        Guid? userId,
        string name,
        string email,
        string password,
        string? accountImageUrl = null,
        string? phoneNumber = null)
    {
        var admin = await FindExistingAdminAsync(userId, email);
        if (admin is null)
        {
            admin = User.Create(name, email, accountImageUrl);
            if (userId.HasValue)
                admin.Id = userId.Value;
            admin.EmailConfirmed = true;
            admin.PhoneNumber = phoneNumber;

            var createResult = await userManager.CreateAsync(admin);
            if (!createResult.Succeeded)
                throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));

            var addPasswordResult = await userManager.AddPasswordAsync(admin, password);
            if(!addPasswordResult.Succeeded)
                throw new Exception(string.Join("; ", addPasswordResult.Errors.Select(e => e.Description)));

            if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
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

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
            await userManager.AddToRoleAsync(admin, UserRoles.Admin);
    }

    public void SeedAdmin(
        Guid? userId,
        string name,
        string email,
        string password,
        string? accountImageUrl = null,
        string? phoneNumber = null)
    {
        var admin = FindExistingAdmin(userId, email);
        var adminRoleId = context.Roles.FirstOrDefault(r => r.Name == UserRoles.Admin)?.Id;
        if (adminRoleId is null)
            throw new InvalidOperationException("Admin role was not found during seeding.");

        if (admin is null)
        {
            admin = User.Create(name, email, accountImageUrl);
            if (userId.HasValue)
                admin.Id = userId.Value;
            context.Add(admin);
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

        var hasAdminRole = context.UserRoles.Any(x => x.RoleId == adminRoleId && x.UserId == admin.Id);
        if (!hasAdminRole)
        {
            context.UserRoles.Add(new IdentityUserRole<Guid>
            {
                RoleId = adminRoleId.Value,
                UserId = admin.Id
            });
        }
    }

    private User? FindExistingAdmin(Guid? userId, string email)
    {
        if (userId.HasValue)
        {
            var byId = context.Users.FirstOrDefault(x => x.Id == userId.Value);
            if (byId is not null)
                return byId;
        }

        return context.Users.FirstOrDefault(x => x.Email == email);
    }

    private async Task<User?> FindExistingAdminAsync(Guid? userId, string email)
    {
        if (userId.HasValue)
        {
            var byId = await context.Users.FirstOrDefaultAsync(x => x.Id == userId.Value);
            if (byId is not null)
                return byId;
        }

        return await context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }
}

using BasicBuildingBlocks.Extensions.Repositories;
using UserService.Data.Repositiroes.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using UserService.Models;

namespace UserService.Data.Repositiroes;

public class UserRepository(ApplicationDbContext context, 
    UserManager<User> userManager) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, bool IsTracking = false)
    {
        return await userManager.Users
            .IsTracking(IsTracking)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email, bool IsTracking = false)
    {
        return await userManager.Users
            .IsTracking(IsTracking)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task CreateUserAsync(User user, string password, string role)
    {
        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
            throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
            throw new Exception(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
    }

    public async Task CreateGoogleUserAsync(User user, string role)
    {
        var createResult = await userManager.CreateAsync(user);

        if (!createResult.Succeeded)
            throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
            throw new Exception(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
    }

    public async Task<string?> GetUserRoleAsync(User user)
    {
        var userRoles = await userManager.GetRolesAsync(user);
        return userRoles.FirstOrDefault();
    }

    public async Task<int> SaveChangesAsync() => await context.SaveChangesAsync();
}


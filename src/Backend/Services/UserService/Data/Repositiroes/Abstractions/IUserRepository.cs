using UserService.Models;

namespace UserService.Data.Repositiroes.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, bool IsTracking = false);
    Task<string?> GetUserRoleAsync(User user);
    Task<User?> GetByEmailAsync(string email, bool IsTracking = false);
    Task CreateUserAsync(User user, string password, string role);
    Task CreateGoogleUserAsync(User user, string role);
    Task<int> SaveChangesAsync();
}
namespace UserService.Services.AdminSeeder.Abstractions;

public interface IAdminSeeder
{
    Task SeedAdminAsync(string name, string email, string password, string? accountImageUrl = null);
    void SeedAdmin(string name, string email, string password, string? accountImageUrl = null);
}   
namespace UserService.Services.AdminSeeder.Abstractions;

public interface IAdminSeeder
{
    Task SeedAdminAsync(
        Guid? userId,
        string name,
        string email,
        string password,
        string? accountImageUrl = null,
        string? phoneNumber = null);
    void SeedAdmin(
        Guid? userId,
        string name,
        string email,
        string password,
        string? accountImageUrl = null,
        string? phoneNumber = null);
}   

using UserService.Services.AdminSeeder.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using UserService.Models.Enums;

namespace UserService.Data;

public static class DatabaseInitialaizer
{
    public static void InitializeTestDatabase(this WebApplication application,
        bool ensureDelete = false)
    {
        var scope = application.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();

        initializer.Initialize(ensureDelete);
    }

    public static async Task InitializeTestDatabaseAsync(this WebApplication application,
        bool ensureDelete = false)
    {
        using var scope = application.Services.CreateAsyncScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();

        await initializer.InitializeAsync(ensureDelete);
    }

    public static DbContextOptionsBuilder UseSeedingAsync(this DbContextOptionsBuilder builder, IServiceProvider provider)
    {
        builder.UseAsyncSeeding(async (context, _, ct) =>
        {
            var initializer = provider.GetRequiredService<ApplicationDatabaseInitializer>();
            var adminSeeder = provider.GetRequiredService<IAdminSeeder>();
        
            await initializer.SeedAsync();
        });

        return builder;
    }

    public static DbContextOptionsBuilder UseSeeding(this DbContextOptionsBuilder builder, IServiceProvider provider)
    {
        builder.UseSeeding((context, _) =>
        {
            var initializer = provider.GetRequiredService<ApplicationDatabaseInitializer>();
            var adminSeeder = provider.GetRequiredService<IAdminSeeder>();

            initializer.Seed();
        });

        return builder;
    }
}

public class ApplicationDatabaseInitializer(ApplicationDbContext context, IAdminSeeder adminSeeder)
{
    public void Initialize(bool ensureDelete = false)
    {
        try
        {
            if (ensureDelete)
            {
                context.Database.EnsureDeleted();
            }
            context.Database.EnsureCreated();
            EnsureEmailVerificationCodesTable();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        }
    }

    public async Task InitializeAsync(bool ensureDelete = false)
    {
        try
        {
            if (ensureDelete)
            {
                await context.Database.EnsureDeletedAsync();
            }
            await context.Database.EnsureCreatedAsync();
            await EnsureEmailVerificationCodesTableAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        }
    }

    private readonly static List<IdentityRole<Guid>> UserRolesSeed = [
        new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Admin", NormalizedName = "ADMIN" },
        new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER" },
        new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Agent", NormalizedName = "MODERATOR" },  
    ];

    public void Seed()
    {
        try
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(UserRolesSeed);
                context.SaveChanges();
            }

            adminSeeder.SeedAdmin("gromahacer", "gromahacer@gmail.com", "12345678", phoneNumber: "+375291112233");
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        }
    }

    //TODO reduce by reflection
    public async Task SeedAsync()
    {
        try
        {
            if (!context.Roles.Any())
            {
                await context.Roles.AddRangeAsync(UserRolesSeed);
                await context.SaveChangesAsync();
            }

            await adminSeeder.SeedAdminAsync("gromahacer", "gromahacer@gmail.com", "12345678", phoneNumber: "+375291112233");
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
        }
    }

    private void EnsureEmailVerificationCodesTable()
    {
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS email_verification_codes (
                id uuid PRIMARY KEY,
                user_id uuid NOT NULL,
                email character varying(256) NOT NULL,
                code_hash character varying(128) NOT NULL,
                expires_at_utc timestamp with time zone NOT NULL,
                created_at_utc timestamp with time zone NOT NULL,
                is_used boolean NOT NULL DEFAULT false
            );
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS ix_email_verification_codes_email
            ON email_verification_codes (email);
            """);

        context.Database.ExecuteSqlRaw("""
            CREATE INDEX IF NOT EXISTS ix_email_verification_codes_user_state
            ON email_verification_codes (user_id, is_used, expires_at_utc);
            """);
    }

    private Task EnsureEmailVerificationCodesTableAsync()
        => context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS email_verification_codes (
                id uuid PRIMARY KEY,
                user_id uuid NOT NULL,
                email character varying(256) NOT NULL,
                code_hash character varying(128) NOT NULL,
                expires_at_utc timestamp with time zone NOT NULL,
                created_at_utc timestamp with time zone NOT NULL,
                is_used boolean NOT NULL DEFAULT false
            );
            CREATE INDEX IF NOT EXISTS ix_email_verification_codes_email
            ON email_verification_codes (email);
            CREATE INDEX IF NOT EXISTS ix_email_verification_codes_user_state
            ON email_verification_codes (user_id, is_used, expires_at_utc);
            """);
}

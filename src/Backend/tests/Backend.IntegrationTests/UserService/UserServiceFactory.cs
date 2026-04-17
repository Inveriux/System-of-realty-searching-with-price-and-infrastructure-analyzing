using System.Data.Common;
using System.Net.Http.Headers;
using Backend.IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserService.Data;
using UserService.Services.EmailVerification.Abstractions;

namespace Backend.IntegrationTests.UserServiceTests;

using UserServiceDbContext = global::ApplicationDbContext;

public sealed class UserServiceFactory : WebApplicationFactory<UserServiceAssemblyMarker>
{
    private SqliteConnection? _connection;
    private bool _initialized;

    internal TestEmailVerificationService EmailVerification { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:DomainJwtAuth:Authority"] = TestJwtTokenFactory.Authority,
                ["Authentication:DomainJwtAuth:Audience"] = TestJwtTokenFactory.Audience,
                ["Authentication:DomainJwtAuth:Key"] = TestJwtTokenFactory.Key,
                ["Authentication:GoogleAuth:ClientId"] = "tests-client",
                ["Authentication:GoogleAuth:ClientSecret"] = "tests-secret",
                ["Authentication:GoogleAuth:CallbackPath"] = "/api/users/signin-google",
                ["ImageService:BaseUrl"] = "http://image-service.test",
                ["Email:Smtp:Host"] = "smtp.test",
                ["Email:Smtp:SenderEmail"] = "tests@example.com",
                ["ConnectionStrings:PostgreSqlConnection"] = "DataSource=:memory:"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<UserServiceDbContext>));
            services.RemoveAll(typeof(DbContextOptions<UserServiceDbContext>));
            services.RemoveAll(typeof(UserServiceDbContext));
            services.RemoveAll(typeof(IEmailVerificationService));

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton<DbConnection>(_connection);
            services.AddDbContext<UserServiceDbContext>((_, options) => options.UseSqlite(_connection));
            services.AddSingleton<IEmailVerificationService>(EmailVerification);
        });
    }

    public async Task EnsureInitializedAsync()
    {
        if (_initialized)
        {
            return;
        }

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseInitializer>();
        await initializer.SeedAsync();

        _initialized = true;
    }

    public async Task<(string Token, string Email)> RegisterAndLoginAsync()
    {
        await EnsureInitializedAsync();

        var email = $"user-{Guid.NewGuid():N}@example.com";
        var password = "strongpass1";
        var client = CreateClient();

        var signUpResponse = await client.PostAsJsonAsync("/api/users/signup", new
        {
            name = "Integration User",
            email,
            password,
            accountImageUrl = (string?)null
        });
        signUpResponse.EnsureSuccessStatusCode();

        var verifyResponse = await client.PostAsJsonAsync("/api/users/email-verification/verify", new
        {
            email,
            code = EmailVerification.GetCode(email)
        });
        verifyResponse.EnsureSuccessStatusCode();

        var signInResponse = await client.PostAsJsonAsync("/api/users/signin", new
        {
            email,
            password
        });
        signInResponse.EnsureSuccessStatusCode();

        var payload = await signInResponse.Content.ReadFromJsonAsync<ResultEnvelope<SignInResponseDto>>();
        return (payload!.Data!.Token, email);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }

    private sealed class SignInResponseDto
    {
        public bool Success { get; init; }
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}

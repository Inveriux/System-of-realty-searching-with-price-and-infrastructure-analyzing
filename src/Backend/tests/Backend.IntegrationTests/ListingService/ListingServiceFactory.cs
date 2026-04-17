using System.Data.Common;
using BasicBuildingBlocks.BusinessResults;
using Backend.IntegrationTests.Common;
using ListingService.Application.Contracts.Services;
using ListingService.Presentation.Dtos.Images;
using ListingService.Presentation.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Backend.IntegrationTests.ListingServiceTests;

using ListingDbContext = ListingService.Infrastructure.Data.ApplicationDbContext;

public sealed class ListingServiceFactory : WebApplicationFactory<ListingServicePresentationAssemblyMarker>
{
    private SqliteConnection? _connection;

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
                ["ImageService:BaseUrl"] = "http://image-service.test",
                ["InfrastructureService:BaseUrl"] = "http://infrastructure-service.test",
                ["SearchService:BaseUrl"] = "http://search-service.test",
                ["CurrencyRate:BaseUrl"] = "http://currency-service.test",
                ["CurrencyRate:UsdRatePath"] = "/rates/usd",
                ["ConnectionStrings:PostgreSqlConnection"] = "DataSource=:memory:"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<ListingDbContext>));
            services.RemoveAll(typeof(DbContextOptions<ListingDbContext>));
            services.RemoveAll(typeof(ListingDbContext));
            services.RemoveAll(typeof(ICurrencyRateService));
            services.RemoveAll(typeof(ListingService.Application.Contracts.Services.IInfrastructureServiceClient));
            services.RemoveAll(typeof(ISearchServiceClient));
            services.RemoveAll(typeof(IImageServiceClient));

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton<DbConnection>(_connection);
            services.AddDbContext<ListingDbContext>((_, options) => options.UseSqlite(_connection));
            services.AddSingleton<ICurrencyRateService, TestCurrencyRateService>();
            services.AddSingleton<ListingService.Application.Contracts.Services.IInfrastructureServiceClient, TestInfrastructureServiceClient>();
            services.AddSingleton<ISearchServiceClient, TestSearchServiceClient>();
            services.AddSingleton<IImageServiceClient, TestImageServiceClient>();
        });
    }

    public HttpClient CreateAuthenticatedClient(Guid? userId = null, string role = "User")
    {
        var client = CreateClient();
        var subjectId = userId ?? Guid.NewGuid();
        var token = TestJwtTokenFactory.CreateToken(subjectId, "listing-user@example.com", "Listing User", role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }

    private sealed class TestCurrencyRateService : ICurrencyRateService
    {
        public Task<CurrencyConversionResult> ConvertBynToUsdAsync(decimal amountByn, CancellationToken cancellationToken)
            => Task.FromResult(new CurrencyConversionResult(amountByn, decimal.Round(amountByn / 3.2m, 2), 3.2m, DateOnly.FromDateTime(DateTime.UtcNow)));
    }

    private sealed class TestInfrastructureServiceClient : ListingService.Application.Contracts.Services.IInfrastructureServiceClient
    {
        public Task<bool> RefreshInfrastructureAsync(Guid locationId, CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<InfrastructureResolvedLocation?> ResolveLocationAsync(string addressText, CancellationToken cancellationToken)
            => Task.FromResult<InfrastructureResolvedLocation?>(new InfrastructureResolvedLocation(Guid.NewGuid(), 53.9m, 27.56m));
    }

    private sealed class TestSearchServiceClient : ISearchServiceClient
    {
        public Task<bool> DeleteListingAsync(Guid listingId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> IndexListingAsync(SearchListingIndexRequest request, CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class TestImageServiceClient : IImageServiceClient
    {
        public Task<Result<IReadOnlyCollection<UploadedImageDto>>> UploadManyAsync(IEnumerable<IFormFile> files, CancellationToken cancellationToken)
            => Task.FromResult(Result.Ok<IReadOnlyCollection<UploadedImageDto>>([]));

        public Task<Result<UploadedImageDto>> UploadSingleAsync(IFormFile file, CancellationToken cancellationToken)
            => Task.FromResult(Result.Ok(new UploadedImageDto
            {
                Url = "https://images.test/uploaded.jpg"
            }));
    }
}

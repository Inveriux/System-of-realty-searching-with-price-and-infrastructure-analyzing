using System.Data.Common;
using Backend.IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PricingAnalyticsService.DTOs;
using PricingAnalyticsService.Repositories.Abstractions;
using PricingAnalyticsService.Services;

namespace Backend.IntegrationTests.PricingAnalyticsTests;

using PricingDbContext = PricingAnalyticsService.Data.ApplicationDbContext;

public sealed class PricingAnalyticsServiceFactory : WebApplicationFactory<PricingAnalyticsServiceAssemblyMarker>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ListingService:BaseUrl"] = "http://listing-service.test",
                ["ConnectionStrings:PostgreSqlConnection"] = "DataSource=:memory:"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<PricingDbContext>));
            services.RemoveAll(typeof(DbContextOptions<PricingDbContext>));
            services.RemoveAll(typeof(PricingDbContext));
            services.RemoveAll(typeof(IPricingAnalyticsRepository));
            services.RemoveAll(typeof(IListingPriceHistoryClient));

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddSingleton<DbConnection>(_connection);
            services.AddDbContext<PricingDbContext>((_, options) => options.UseSqlite(_connection));
            services.AddSingleton<IPricingAnalyticsRepository, TestPricingAnalyticsRepository>();
            services.AddSingleton<IListingPriceHistoryClient, TestListingPriceHistoryClient>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }

    private sealed class TestPricingAnalyticsRepository : IPricingAnalyticsRepository
    {
        public Task<IReadOnlyCollection<PricingAnalyticsCategoryDto>> GetCategoriesAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<PricingAnalyticsCategoryDto>>([
                new PricingAnalyticsCategoryDto
                {
                    CategoryKey = "apartment",
                    CategoryLabel = "Квартиры",
                    ListingsCount = 4,
                    AveragePricePerSqm = 3200m,
                    MedianPricePerSqm = 3150m,
                    HasEnoughData = true,
                    ConfidenceLevel = "high"
                }
            ]);

        public Task<PricingEstimateResponseDto> GetEstimateAsync(PricingEstimateRequestDto request, CancellationToken cancellationToken)
            => Task.FromResult(new PricingEstimateResponseDto
            {
                EnteredPricePerSqm = request.AreaTotal <= 0 ? null : decimal.Round(request.PriceAmountByn / request.AreaTotal, 2),
                CityAveragePricePerSqm = 3200m,
                CountryAveragePricePerSqm = 2800m,
                LocalRadiusAveragePricePerSqm = 3300m,
                CityListingsCount = 12,
                CountryListingsCount = 64,
                LocalRadiusListingsCount = 6,
                RelativePosition = "near_market",
                Message = "Цена близка к рынку.",
                ConfidenceLevel = "high",
                HasEnoughData = true
            });

        public Task<ListingPriceComparisonDto?> GetListingComparisonAsync(Guid listingId, CancellationToken cancellationToken)
            => Task.FromResult<ListingPriceComparisonDto?>(new ListingPriceComparisonDto
            {
                CurrentListingPricePerSqm = 3100m,
                CityAveragePricePerSqm = 3200m,
                RelativePosition = "below_market",
                Message = "Цена немного ниже среднего.",
                HasEnoughData = true,
                ConfidenceLevel = "medium"
            });

        public Task<PricingAnalyticsSummaryDto> GetSummaryAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken)
            => Task.FromResult(new PricingAnalyticsSummaryDto
            {
                AveragePricePerSqm = 3000m,
                MedianPricePerSqm = 2950m,
                ListingsCount = 10,
                HasEnoughData = true,
                ConfidenceLevel = "high"
            });

        public Task<IReadOnlyCollection<PricingAnalyticsPointDto>> GetTimeSeriesAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<PricingAnalyticsPointDto>>([
                new PricingAnalyticsPointDto
                {
                    Period = "2026-01",
                    AveragePricePerSqm = 3000m,
                    MedianPricePerSqm = 2950m,
                    ListingsCount = 5,
                    HasEnoughData = true,
                    ConfidenceLevel = "high"
                }
            ]);
    }

    private sealed class TestListingPriceHistoryClient : IListingPriceHistoryClient
    {
        public Task<ListingPriceHistoryDto?> AddPriceHistoryEntryAsync(Guid listingId, CreateListingPriceHistoryEntryRequestDto request, CancellationToken cancellationToken)
            => Task.FromResult<ListingPriceHistoryDto?>(new ListingPriceHistoryDto { ListingId = listingId });

        public Task<ListingPriceHistoryDto?> DeletePriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, CancellationToken cancellationToken)
            => Task.FromResult<ListingPriceHistoryDto?>(new ListingPriceHistoryDto { ListingId = listingId });

        public Task<ListingPriceHistoryDto?> GetPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken)
            => Task.FromResult<ListingPriceHistoryDto?>(new ListingPriceHistoryDto { ListingId = listingId });

        public Task<ListingPriceHistoryDto?> UpdatePriceAsync(Guid listingId, UpdateListingPriceRequestDto request, CancellationToken cancellationToken)
            => Task.FromResult<ListingPriceHistoryDto?>(new ListingPriceHistoryDto { ListingId = listingId });

        public Task<ListingPriceHistoryDto?> UpdatePriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, UpdateListingPriceHistoryEntryRequestDto request, CancellationToken cancellationToken)
            => Task.FromResult<ListingPriceHistoryDto?>(new ListingPriceHistoryDto { ListingId = listingId });
    }
}

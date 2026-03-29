using PricingAnalyticsService.DTOs;

namespace PricingAnalyticsService.Repositories.Abstractions;

public interface IPricingAnalyticsRepository
{
    Task<PricingAnalyticsSummaryDto> GetSummaryAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PricingAnalyticsPointDto>> GetTimeSeriesAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<PricingAnalyticsCategoryDto>> GetCategoriesAsync(PricingAnalyticsQueryDto query, CancellationToken cancellationToken);
    Task<ListingPriceComparisonDto?> GetListingComparisonAsync(Guid listingId, CancellationToken cancellationToken);
    Task<PricingEstimateResponseDto> GetEstimateAsync(PricingEstimateRequestDto request, CancellationToken cancellationToken);
}

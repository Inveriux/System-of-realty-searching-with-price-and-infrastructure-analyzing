namespace PricingAnalyticsService.DTOs;

public sealed class PricingAnalyticsQueryDto
{
    public string? ListingType { get; init; }
    public string? Geography { get; init; }
    public string? City { get; init; }
    public string? BuildingType { get; init; }
    public string? ListingDetailsType { get; init; }
    public int? Months { get; init; }
}

public sealed class PricingAnalyticsSummaryDto
{
    public decimal? AveragePricePerSqm { get; init; }
    public decimal? MedianPricePerSqm { get; init; }
    public decimal? MinPricePerSqm { get; init; }
    public decimal? MaxPricePerSqm { get; init; }
    public decimal? MarketRangeLowPricePerSqm { get; init; }
    public decimal? MarketRangeHighPricePerSqm { get; init; }
    public int ListingsCount { get; init; }
    public DateTimeOffset? PeriodStart { get; init; }
    public DateTimeOffset? PeriodEnd { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string ConfidenceLevel { get; init; } = "low";
    public bool HasEnoughData { get; init; }
    public string TrendDirection { get; init; } = "flat";
}

public sealed class PricingAnalyticsPointDto
{
    public string Period { get; init; } = string.Empty;
    public decimal? AveragePricePerSqm { get; init; }
    public decimal? MedianPricePerSqm { get; init; }
    public int ListingsCount { get; init; }
    public bool HasEnoughData { get; init; }
    public string ConfidenceLevel { get; init; } = "low";
}

public sealed class PricingAnalyticsCategoryDto
{
    public string CategoryKey { get; init; } = string.Empty;
    public string CategoryLabel { get; init; } = string.Empty;
    public decimal? AveragePricePerSqm { get; init; }
    public decimal? MedianPricePerSqm { get; init; }
    public decimal? MinPricePerSqm { get; init; }
    public decimal? MaxPricePerSqm { get; init; }
    public decimal? MarketRangeLowPricePerSqm { get; init; }
    public decimal? MarketRangeHighPricePerSqm { get; init; }
    public int ListingsCount { get; init; }
    public string TrendDirection { get; init; } = "flat";
    public string ConfidenceLevel { get; init; } = "low";
    public bool HasEnoughData { get; init; }
}

public sealed class ListingPriceComparisonDto
{
    public decimal? CurrentListingPricePerSqm { get; init; }
    public decimal? LocalRadiusAveragePricePerSqm { get; init; }
    public decimal? LocalRadiusMedianPricePerSqm { get; init; }
    public int LocalRadiusListingsCount { get; init; }
    public decimal? CityAveragePricePerSqm { get; init; }
    public decimal? CityMedianPricePerSqm { get; init; }
    public int CityListingsCount { get; init; }
    public decimal? CountryAveragePricePerSqm { get; init; }
    public decimal? CountryMedianPricePerSqm { get; init; }
    public int CountryListingsCount { get; init; }
    public decimal? MarketRangeLowPricePerSqm { get; init; }
    public decimal? MarketRangeHighPricePerSqm { get; init; }
    public string RelativePosition { get; init; } = "no_data";
    public string Message { get; init; } = "Нет информации";
    public string ConfidenceLevel { get; init; } = "low";
    public bool HasEnoughData { get; init; }
}

public sealed class PricingEstimateRequestDto
{
    public string? ListingType { get; init; }
    public string? BuildingType { get; init; }
    public string? ListingDetailsType { get; init; }
    public decimal PriceAmountByn { get; init; }
    public decimal AreaTotal { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? CityName { get; init; }
    public Guid? LocationId { get; init; }
}

public sealed class PricingEstimateResponseDto
{
    public decimal? EnteredPricePerSqm { get; init; }
    public decimal? LocalRadiusAveragePricePerSqm { get; init; }
    public decimal? CityAveragePricePerSqm { get; init; }
    public decimal? CountryAveragePricePerSqm { get; init; }
    public int LocalRadiusListingsCount { get; init; }
    public int CityListingsCount { get; init; }
    public int CountryListingsCount { get; init; }
    public string RelativePosition { get; init; } = "no_data";
    public string Message { get; init; } = "Нет информации";
    public string ConfidenceLevel { get; init; } = "low";
    public bool HasEnoughData { get; init; }
}

public sealed class ListingPriceHistoryPointDto
{
    public Guid Id { get; init; }
    public DateTimeOffset EffectiveDateUtc { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PricePerSqmByn { get; init; }
    public string? Comment { get; init; }
}

public sealed class ListingPriceHistoryDto
{
    public Guid ListingId { get; init; }
    public decimal? CurrentPriceAmountByn { get; init; }
    public decimal? FirstRecordedPriceAmountByn { get; init; }
    public decimal? LastRecordedPriceAmountByn { get; init; }
    public DateTimeOffset? FirstRecordedAt { get; init; }
    public DateTimeOffset? LastRecordedAt { get; init; }
    public decimal? PercentChangeFromFirst { get; init; }
    public IReadOnlyCollection<ListingPriceHistoryPointDto> History { get; init; } = [];
}

public sealed class UpdateListingPriceRequestDto
{
    public decimal PriceAmountByn { get; init; }
    public DateTimeOffset? EffectiveDateUtc { get; init; }
    public string? Comment { get; init; }
}

public sealed class CreateListingPriceHistoryEntryRequestDto
{
    public decimal PriceAmountByn { get; init; }
    public DateTimeOffset EffectiveDateUtc { get; init; }
    public string? Comment { get; init; }
}

public sealed class UpdateListingPriceHistoryEntryRequestDto
{
    public decimal? PriceAmountByn { get; init; }
    public DateTimeOffset? EffectiveDateUtc { get; init; }
    public string? Comment { get; init; }
}

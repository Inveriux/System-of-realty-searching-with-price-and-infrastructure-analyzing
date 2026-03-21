using System.Text.Json;

namespace ListingPageBff.Contracts.ListingPages;

public sealed class ListingPageDto
{
    public required ListingPageListingDto Listing { get; init; }
    public ListingPageInfrastructureDto? Infrastructure { get; init; }
    public required ListingPageSellerDto Seller { get; init; }
    public required ListingPageDerivedDto Derived { get; init; }
    public required ListingPageAvailableBlocksDto AvailableBlocks { get; init; }
    public IReadOnlyCollection<ListingPageWarningDto> Warnings { get; init; } = [];
}

public sealed class ListingPageListingDto
{
    public Guid Id { get; init; }
    public Guid AgentId { get; init; }
    public Guid? LocationId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AddressText { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public decimal? PriceAmount { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
    public string? PricePeriod { get; init; }
    public decimal AreaTotal { get; init; }
    public int? BuildYear { get; init; }
    public DateTimeOffset? PublishedDate { get; init; }
    public string? BuildingType { get; init; }
    public string ListingType { get; init; } = string.Empty;
    public string? MainMedia { get; init; }
    public IReadOnlyCollection<ListingPageMediaDto> Media { get; init; } = [];
    public string? ListingDetailsType { get; init; }
    public JsonElement? ListingDetails { get; init; }
    public IReadOnlyCollection<ListingPagePriceHistoryDto> PriceHistory { get; init; } = [];
}

public sealed class ListingPageMediaDto
{
    public string Url { get; init; } = string.Empty;
}

public sealed class ListingPagePriceHistoryDto
{
    public DateTimeOffset ChangedAt { get; init; }
    public decimal? PriceAmount { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
}

public sealed class ListingPageInfrastructureDto
{
    public decimal? Score { get; init; }
    public decimal? TotalScore { get; init; }
    public decimal? EducationScore { get; init; }
    public decimal? HealthcareScore { get; init; }
    public decimal? TransportScore { get; init; }
    public decimal? CommerceScore { get; init; }
    public decimal? LeisureScore { get; init; }
    public int PoiCount { get; init; }
    public string? Summary { get; init; }
    public IReadOnlyCollection<ListingPageGroupedPoisDto> GroupedPois { get; init; } = [];
    public IReadOnlyCollection<ListingPagePoiDto> NearbyPois { get; init; } = [];
    public IReadOnlyCollection<string> Highlights { get; init; } = [];
    public DateTime? CalculatedAtUtc { get; init; }
}

public sealed class ListingPagePoiDto
{
    public Guid Id { get; init; }
    public string? ExternalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? AddressText { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string Category { get; init; } = string.Empty;
    public decimal DistanceMeters { get; init; }
}

public sealed class ListingPageGroupedPoisDto
{
    public string Category { get; init; } = string.Empty;
    public int Count { get; init; }
    public IReadOnlyCollection<ListingPagePoiDto> Pois { get; init; } = [];
}

public sealed class ListingPageSellerDto
{
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public decimal? Rating { get; init; }
    public int? ReviewsCount { get; init; }
    public bool? IsVerified { get; init; }
    public string? AgencyName { get; init; }
    public bool IsFallback { get; init; }
}

public sealed class ListingPageDerivedDto
{
    public IReadOnlyCollection<ListingQuickStatDto> QuickStats { get; init; } = [];
    public IReadOnlyCollection<ListingFeatureTagDto> FeatureTags { get; init; } = [];
    public IReadOnlyCollection<string> Warnings { get; init; } = [];
    public DateTimeOffset LastUpdatedUtc { get; init; }
}

public sealed class ListingQuickStatDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Unit { get; init; }
}

public sealed class ListingFeatureTagDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Tone { get; init; }
}

public sealed class ListingPageWarningDto
{
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Severity { get; init; } = "warning";
    public string? Source { get; init; }
}

public sealed class ListingPageAvailableBlocksDto
{
    public bool Listing { get; init; }
    public bool Infrastructure { get; init; }
    public bool Seller { get; init; }
    public bool PriceHistory { get; init; }
    public bool Map { get; init; }
}

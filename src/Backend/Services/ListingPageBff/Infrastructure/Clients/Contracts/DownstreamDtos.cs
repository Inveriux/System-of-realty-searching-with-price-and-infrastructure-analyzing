using System.Text.Json;

namespace ListingPageBff.Infrastructure.Clients.Contracts;

public sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
}

public sealed class DownstreamDetailedListingDto
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
    public IReadOnlyCollection<DownstreamListingMediaDto> Media { get; init; } = [];
    public IReadOnlyCollection<DownstreamListingPriceHistoryDto> PriceHistory { get; init; } = [];
    public string? ListingDetailsType { get; init; }
    public JsonElement? ListingDetails { get; init; }
}

public sealed class DownstreamListingMediaDto
{
    public string Url { get; init; } = string.Empty;
}

public sealed class DownstreamListingPriceHistoryDto
{
    public DateTimeOffset ChangedAt { get; init; }
    public decimal? PriceAmount { get; init; }
    public string? PriceCurrency { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PriceAmountUsd { get; init; }
}

public sealed class DownstreamInfrastructureSummaryDto
{
    public Guid LocationId { get; init; }
    public string AddressText { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public DownstreamInfrastructureScoreDto Score { get; init; } = new();
    public string? Summary { get; init; }
    public IReadOnlyCollection<string> Highlights { get; init; } = [];
    public IReadOnlyCollection<DownstreamPoiDto> Pois { get; init; } = [];
}

public sealed class DownstreamRefreshInfrastructureRequestDto
{
    public Guid? LocationId { get; init; }
    public string? AddressText { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public int SearchRadiusMeters { get; init; } = 1500;
}

public sealed class DownstreamInfrastructureScoreDto
{
    public Guid LocationId { get; init; }
    public decimal TotalScore { get; init; }
    public decimal EducationScore { get; init; }
    public decimal HealthcareScore { get; init; }
    public decimal TransportScore { get; init; }
    public decimal CommerceScore { get; init; }
    public decimal LeisureScore { get; init; }
    public int PoiCount { get; init; }
    public DateTime CalculatedAtUtc { get; init; }
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];
}

public sealed class DownstreamPoiDto
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

public sealed class DownstreamSellerPublicProfileDto
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
}

public sealed class DownstreamSearchListingDto
{
    public Guid ListingId { get; init; }
    public decimal? InfrastructureScore { get; init; }
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];
}

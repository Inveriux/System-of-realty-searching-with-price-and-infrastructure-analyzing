namespace SearchService.DTOs.Search;

public sealed class SearchListingsRequestDto
{
    public string? Query { get; init; }
    public Guid? LocationId { get; init; }
    public string? ListingType { get; init; }
    public string? BuildingType { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? MinArea { get; init; }
    public decimal? MaxArea { get; init; }
    public decimal? MinInfrastructureScore { get; init; }
    public decimal? MinEducationScore { get; init; }
    public decimal? MinHealthcareScore { get; init; }
    public decimal? MinTransportScore { get; init; }
    public decimal? MinCommerceScore { get; init; }
    public decimal? MinLeisureScore { get; init; }
    public int? MinEducationCount { get; init; }
    public int? MinHealthcareCount { get; init; }
    public int? MinTransportCount { get; init; }
    public int? MinCommerceCount { get; init; }
    public int? MinLeisureCount { get; init; }
    public decimal? MaxEducationNearestMeters { get; init; }
    public decimal? MaxHealthcareNearestMeters { get; init; }
    public decimal? MaxTransportNearestMeters { get; init; }
    public decimal? MaxCommerceNearestMeters { get; init; }
    public decimal? MaxLeisureNearestMeters { get; init; }
    public decimal? CenterLatitude { get; init; }
    public decimal? CenterLongitude { get; init; }
    public decimal? RadiusMeters { get; init; }
    public decimal? BoundsMinLatitude { get; init; }
    public decimal? BoundsMinLongitude { get; init; }
    public decimal? BoundsMaxLatitude { get; init; }
    public decimal? BoundsMaxLongitude { get; init; }
    public string? SortBy { get; init; }
    public int Page { get; init; } = 0;
    public int PageSize { get; init; } = 20;
}

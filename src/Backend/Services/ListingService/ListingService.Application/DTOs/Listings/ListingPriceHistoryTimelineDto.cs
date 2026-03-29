namespace ListingService.Application.DTOs.Listings;

public sealed class ListingPriceHistoryTimelinePointDto
{
    public Guid Id { get; init; }
    public DateTimeOffset EffectiveDateUtc { get; init; }
    public decimal? PriceAmountByn { get; init; }
    public decimal? PricePerSqmByn { get; init; }
    public string? Comment { get; init; }
}

public sealed class ListingPriceHistoryTimelineDto
{
    public Guid ListingId { get; init; }
    public decimal? CurrentPriceAmountByn { get; init; }
    public decimal? FirstRecordedPriceAmountByn { get; init; }
    public decimal? LastRecordedPriceAmountByn { get; init; }
    public DateTimeOffset? FirstRecordedAt { get; init; }
    public DateTimeOffset? LastRecordedAt { get; init; }
    public decimal? PercentChangeFromFirst { get; init; }
    public IReadOnlyCollection<ListingPriceHistoryTimelinePointDto> History { get; init; } = [];
}

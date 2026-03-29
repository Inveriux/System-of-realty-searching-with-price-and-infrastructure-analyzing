namespace ListingService.Application.DTOs.Listings;

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

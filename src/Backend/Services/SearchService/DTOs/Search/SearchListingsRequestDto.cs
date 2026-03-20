namespace SearchService.DTOs.Search;

public sealed class SearchListingsRequestDto
{
    public string? Query { get; init; }
    public Guid? LocationId { get; init; }
    public string? ListingType { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int Page { get; init; } = 0;
    public int PageSize { get; init; } = 20;
}

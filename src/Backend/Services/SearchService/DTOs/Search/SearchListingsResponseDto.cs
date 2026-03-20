namespace SearchService.DTOs.Search;

public sealed class SearchListingsResponseDto
{
    public IReadOnlyCollection<SearchListingDto> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalCount { get; init; }
}

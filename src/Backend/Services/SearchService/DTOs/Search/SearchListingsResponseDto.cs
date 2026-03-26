namespace SearchService.DTOs.Search;

public sealed class SearchListingsResponseDto
{
    public IReadOnlyCollection<SearchListingDto> Items { get; init; } = [];
    public string Sort { get; init; } = "published_desc";
    public string? SearchProfile { get; init; }
    public SearchRankingWeightsDto? AppliedWeights { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalCount { get; init; }
}

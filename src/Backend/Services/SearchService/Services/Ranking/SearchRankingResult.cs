using SearchService.DTOs.Search;

namespace SearchService.Services.Ranking;

public sealed class SearchRankingResult
{
    public decimal FinalScore { get; init; }
    public SearchRelevanceBreakdownDto Breakdown { get; init; } = new();
    public IReadOnlyCollection<string> Highlights { get; init; } = [];
}

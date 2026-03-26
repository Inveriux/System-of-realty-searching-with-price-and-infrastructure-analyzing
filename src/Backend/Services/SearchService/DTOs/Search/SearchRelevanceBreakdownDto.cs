namespace SearchService.DTOs.Search;

public sealed class SearchRelevanceBreakdownDto
{
    public decimal BaseMatchScore { get; init; }
    public decimal PriceFitScore { get; init; }
    public decimal AreaFitScore { get; init; }
    public decimal LocationFitScore { get; init; }
    public decimal InfrastructureFitScore { get; init; }
    public decimal PreferenceAlignmentScore { get; init; }
    public decimal FreshnessScore { get; init; }
    public decimal FinalScore { get; init; }
}

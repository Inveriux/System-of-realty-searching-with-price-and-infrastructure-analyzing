namespace InfrastructureService.DTOs.Infrastructure;

public sealed class CategoryScoreBreakdownDto
{
    public decimal Score { get; init; }
    public decimal ProximityScore { get; init; }
    public decimal DensityScore { get; init; }
    public decimal DiversityScore { get; init; }
    public decimal CompletenessBonus { get; init; }
    public decimal? AppliedCap { get; init; }
    public string? AppliedCapReason { get; init; }
    public bool CompletenessAchieved { get; init; }
    public IReadOnlyCollection<string> PresentSubcategories { get; init; } = [];
    public IReadOnlyCollection<string> MissingImportantSubcategories { get; init; } = [];
    public IReadOnlyCollection<string> PresentBlocks { get; init; } = [];
    public IReadOnlyCollection<string> MissingBlocks { get; init; } = [];
    public Dictionary<string, decimal> ScoreByBlock { get; init; } = [];
    public Dictionary<string, decimal> ScoreBySubcategory { get; init; } = [];
    public Dictionary<string, int> GroupedCounts { get; init; } = [];
    public Dictionary<string, string> DebugInfo { get; init; } = [];
}

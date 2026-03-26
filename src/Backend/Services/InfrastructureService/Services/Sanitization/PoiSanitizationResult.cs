namespace InfrastructureService.Services.Sanitization;

public sealed class PoiSanitizationResult
{
    public bool ShouldExclude { get; init; }
    public string? Reason { get; init; }
    public decimal AddressLikenessScore { get; init; }
    public decimal PoiConfidenceScore { get; init; }
}

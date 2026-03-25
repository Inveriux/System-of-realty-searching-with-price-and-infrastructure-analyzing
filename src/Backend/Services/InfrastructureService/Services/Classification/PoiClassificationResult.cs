namespace InfrastructureService.Services.Classification;

public sealed class PoiClassificationResult
{
    public string CategoryCode { get; init; } = string.Empty;
    public string? SubcategoryCode { get; init; }
    public string Source { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
}

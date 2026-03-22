namespace SearchService.Services.InternalClients.Contracts;

public sealed class InfrastructureScorePayloadDto
{
    public Guid LocationId { get; init; }
    public decimal TotalScore { get; init; }
    public decimal EducationScore { get; init; }
    public decimal HealthcareScore { get; init; }
    public decimal TransportScore { get; init; }
    public decimal CommerceScore { get; init; }
    public decimal LeisureScore { get; init; }
    public int PoiCount { get; init; }
    public DateTime CalculatedAtUtc { get; init; }
    public Dictionary<string, decimal> SearchFeatures { get; init; } = [];
}

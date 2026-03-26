namespace InfrastructureService.Services.Scoring.ScoringEntities;

public sealed class InfrastructureScoringInput
{
    public Dictionary<string, IReadOnlyCollection<InfrastructureScoringEntity>> CategoryEntities { get; } = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<InfrastructureScoringEntity> GetEntities(string categoryCode)
        => CategoryEntities.TryGetValue(categoryCode, out var entities) ? entities : [];
}

namespace InfrastructureService.Models;

public sealed class InfrastructureScore
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public Guid SnapshotId { get; set; }
    public LocationPoiSnapshot? Snapshot { get; set; }
    public decimal TotalScore { get; set; }
    public decimal EducationScore { get; set; }
    public decimal HealthcareScore { get; set; }
    public decimal TransportScore { get; set; }
    public decimal CommerceScore { get; set; }
    public decimal LeisureScore { get; set; }
    public int PoiCount { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
    public string SearchFeaturesJson { get; set; } = "{}";
}

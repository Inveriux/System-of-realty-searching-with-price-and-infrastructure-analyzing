namespace InfrastructureService.Models;

public sealed class LocationPoiSnapshot
{
    public Guid Id { get; set; }
    public Guid LocationId { get; set; }
    public Location? Location { get; set; }
    public int SearchRadiusMeters { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime RefreshedAtUtc { get; set; }
    public ICollection<LocationPoiSnapshotItem> Items { get; set; } = [];
    public InfrastructureScore? InfrastructureScore { get; set; }
}

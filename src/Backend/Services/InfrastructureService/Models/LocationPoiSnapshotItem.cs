namespace InfrastructureService.Models;

public sealed class LocationPoiSnapshotItem
{
    public Guid Id { get; set; }
    public Guid SnapshotId { get; set; }
    public LocationPoiSnapshot? Snapshot { get; set; }
    public Guid PoiId { get; set; }
    public Poi? Poi { get; set; }
    public decimal DistanceMeters { get; set; }
}

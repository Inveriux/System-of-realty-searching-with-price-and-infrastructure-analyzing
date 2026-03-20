namespace InfrastructureService.Models;

public sealed class Location
{
    public Guid Id { get; set; }
    public string AddressText { get; set; } = string.Empty;
    public string NormalizedAddressText { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? HouseNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public ICollection<LocationPoiSnapshot> Snapshots { get; set; } = [];
}

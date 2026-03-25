namespace InfrastructureService.Models;

public sealed class Poi
{
    public Guid Id { get; set; }
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AddressText { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public Guid CategoryId { get; set; }
    public PoiCategory? Category { get; set; }
    public string? SubcategoryCode { get; set; }
    public string? Source { get; set; }
}

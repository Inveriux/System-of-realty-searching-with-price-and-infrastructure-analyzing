namespace InfrastructureService.Models;

public sealed class PoiCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SearchText { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ICollection<Poi> Pois { get; set; } = [];
}

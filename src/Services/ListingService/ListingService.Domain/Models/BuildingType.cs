using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;

public class BuildingType : ILookupEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

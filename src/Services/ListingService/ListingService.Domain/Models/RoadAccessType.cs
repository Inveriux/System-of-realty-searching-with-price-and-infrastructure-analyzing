using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;


public class RoadAccessType : ILookupEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

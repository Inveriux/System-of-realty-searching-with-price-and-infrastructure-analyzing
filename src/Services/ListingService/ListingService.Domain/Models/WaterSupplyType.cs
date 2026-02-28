using ListingService.Domain.Abstractions;

namespace ListingService.Domain.Models;


public class WaterSupplyType : ILookupEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

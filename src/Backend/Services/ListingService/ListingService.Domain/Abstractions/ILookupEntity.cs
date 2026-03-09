namespace ListingService.Domain.Abstractions;

public interface ILookupEntity : IEntity<Guid>
{
    string Name { get; set; }
}

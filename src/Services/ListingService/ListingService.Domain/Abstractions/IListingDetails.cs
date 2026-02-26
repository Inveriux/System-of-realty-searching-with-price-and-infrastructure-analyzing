using ListingService.Domain.Models;

namespace ListingService.Domain.Abstractions;

public interface IListingDetails
{
    public Guid ListingId { get; set; }
    public Listing? Listing { get; set; }
}
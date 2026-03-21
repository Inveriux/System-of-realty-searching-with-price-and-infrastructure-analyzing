using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients.Abstractions;

public interface IListingServiceClient
{
    Task<DownstreamDetailedListingDto?> GetDetailedListingAsync(Guid listingId, CancellationToken cancellationToken);
}

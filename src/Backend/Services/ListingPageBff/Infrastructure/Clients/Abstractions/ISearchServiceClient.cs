using ListingPageBff.Infrastructure.Clients.Contracts;

namespace ListingPageBff.Infrastructure.Clients.Abstractions;

public interface ISearchServiceClient
{
    Task<DownstreamSearchListingDto?> GetListingAsync(Guid listingId, CancellationToken cancellationToken);
}

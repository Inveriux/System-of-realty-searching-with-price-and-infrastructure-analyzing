namespace ChatService.Clients.Listings.Abstractions;

public interface IListingCatalogClient
{
    Task<ListingSummarySnapshot?> GetListingAsync(Guid listingId, CancellationToken cancellationToken);
}

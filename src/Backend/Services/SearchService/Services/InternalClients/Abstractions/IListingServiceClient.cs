using SearchService.DTOs.Internal;

namespace SearchService.Services.InternalClients.Abstractions;

public interface IListingServiceClient
{
    Task<IndexListingRequestDto?> GetListingAsync(Guid listingId, CancellationToken cancellationToken);
}

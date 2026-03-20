using BasicBuildingBlocks.Collections;
using SearchService.Models;

namespace SearchService.Data.Repositories.Abstractions;

public interface ISearchListingRepository
{
    Task<SearchListing?> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SearchListing>> GetByLocationIdAsync(Guid locationId, CancellationToken cancellationToken);
    Task<PagedResult<SearchListing>> SearchAsync(
        string? query,
        Guid? locationId,
        string? listingType,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
    Task UpsertAsync(SearchListing listing, CancellationToken cancellationToken);
    void Remove(SearchListing listing);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

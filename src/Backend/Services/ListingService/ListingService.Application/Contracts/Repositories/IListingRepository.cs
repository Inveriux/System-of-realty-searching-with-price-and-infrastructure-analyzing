using BasicBuildingBlocks.Collections;
using ListingService.Domain.Abstractions;
using ListingService.Domain.Models;

namespace ListingService.Application.Contracts.Repositories;

public interface IListingRepository
{
    Task<PagedResult<Listing>> GetAllAsync(PagedRequest pageRequest, 
        bool isTracking = true, 
        CancellationToken cancellationToken = default);
    Task<PagedResult<Listing>> GetAllDetailedAsync(PagedRequest pageRequest,
        bool isTracking = false,
        CancellationToken cancellationToken = default);
    Task<Listing?> GetByIdAsync(Guid id, bool isTracking, CancellationToken cancellationToken);
    Task<Listing?> GetDetailedByIdAsync(Guid id, bool isTracking = false, CancellationToken cancellationToken = default);
    Task AddAsync(Listing listing, CancellationToken cancellationToken);
    Task ReplaceDetailsAsync(Guid listingId, string listingDetailsType, IListingDetails details, CancellationToken cancellationToken);
    Task ChangePriceHistoryAsync(Guid listingId, ListingPriceHistory newPriceHistory, CancellationToken cancellationToken);
    Task<ListingPriceHistory?> GetLastPriceAsync(Guid listingId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ListingPriceHistory>> GetPriceHistoryAsync(Guid listingId, CancellationToken cancellationToken);
    Task<ListingPriceHistory?> GetPriceHistoryEntryAsync(Guid listingId, Guid historyEntryId, bool isTracking, CancellationToken cancellationToken);
    Task AddPriceHistoryEntryAsync(ListingPriceHistory entry, CancellationToken cancellationToken);
    void RemovePriceHistoryEntry(ListingPriceHistory entry);
    Task SyncCurrentPriceWithLatestHistoryAsync(Guid listingId, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

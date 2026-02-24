using ListingService.Domain.Models;
using BasicBuildingBlocks.Collections;

namespace ListingService.Application.Contracts.Repositories;

public interface IListingRepository
{
    Task<PagedResult<Listing>> GetAllAsync(PagedRequest pageRequest, 
        bool isTracking = true, 
        CancellationToken cancellationToken = default);
    Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Listing listing, CancellationToken cancellationToken);
    Task UpdateAsync(Listing listing, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    bool SaveChanges();
}
using ListingService.Application.Contracts.Repositories;
using ListingService.Domain.Abstractions;

public interface IRepositoryManager
{
    IListingRepository Listing { get; }
    ILookupRepository<TLookup> GetLookup<TLookup>() where TLookup : class, ILookupEntity;
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    bool SaveChanges();
}
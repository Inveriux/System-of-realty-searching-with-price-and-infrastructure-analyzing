using ListingService.Application.Contracts.Repositories;

public interface IRepositoryManager
{
    IListingRepository Listing { get; }

    ILookupRepository<Lookup> GetLookup<Lookup>() where Lookup : class;

    Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
    bool SaveChanges();
}
using ListingService.Domain.Abstractions;
using BasicBuildingBlocks.Collections;

namespace ListingService.Application.Contracts.Repositories;

public interface ILookupRepository<TLookup> where TLookup : class, ILookupEntity
{
    Task<PagedResult<TLookup>> GetAllAsync<TKey>(PagedRequest pagedRequest, CancellationToken cancellationToken);
    Task<List<TLookup>> GetAllAsync<TKey>(CancellationToken cancellationToken);
    PagedResult<TLookup> GetAll<TKey>(PagedRequest pagedRequest);
    List<TLookup> GetAll<TKey>();
    Task<TLookup?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<TLookup?> GetByIdAsync(object id, CancellationToken cancellationToken);
    Task<TLookup> AddAsync(TLookup entity, CancellationToken cancellationToken = default);
    TLookup Update(TLookup entity);
    void Delete(TLookup entity);
    Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool SaveChanges();
}
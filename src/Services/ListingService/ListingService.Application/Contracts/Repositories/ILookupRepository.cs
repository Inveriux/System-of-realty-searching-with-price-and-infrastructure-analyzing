using BasicBuildingBlocks.Pages;

namespace ListingService.Application.Contracts.Repositories;

public interface ILookupRepository<TEntity> where TEntity : class
{
    Task<PagedResult<TEntity>> GetAllAsync<TKey>(CancellationToken cancellationToken);
    Task<TEntity?> GetByNameAsync(string name, CancellationToken cancellationToken);
    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    TEntity Update(TEntity entity);
    void Delete(TEntity entity);
    Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    bool SaveChanges();
}
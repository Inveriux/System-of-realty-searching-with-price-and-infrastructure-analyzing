using ListingService.Application.Contracts.Repositories;
using BasicBuildingBlocks.Repositories;
using BasicBuildingBlocks.Pages;

namespace ListingService.Infrastructure.Data.Repositories;

public class LookupRepository<TEntity>(ApplicationDbContext context) : BaseRepository<TEntity>(context),
    ILookupRepository<TEntity>
    where TEntity : class
{
    public Task<PagedResult<TEntity>> GetAllAsync<TKey>(CancellationToken cancellationToken)
        => GetAllAsync<TKey>(new(0,0), null, false, false, cancellationToken);
}
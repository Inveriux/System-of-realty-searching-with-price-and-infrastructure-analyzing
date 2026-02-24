using ListingService.Application.Contracts.Repositories;
using BasicBuildingBlocks.Repositories;
using Microsoft.EntityFrameworkCore;
using BasicBuildingBlocks.Collections;

namespace ListingService.Infrastructure.Data.Repositories;

public class LookupRepository<TEntity>(ApplicationDbContext context) : BaseRepository<TEntity>(context),
    ILookupRepository<TEntity>
    where TEntity : class
{
    public Task<PagedResult<TEntity>> GetAllAsync<TKey>(CancellationToken cancellationToken)
        => GetAllAsync<TKey>(new(0,0), null, false, false, cancellationToken);

    public Task<TEntity?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => context.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name, cancellationToken);

    public async Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await context.Set<TEntity>()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name, cancellationToken);

        if (entity is null) 
            return false;

        context.Set<TEntity>().Remove(entity);  
        return true;
    }
}

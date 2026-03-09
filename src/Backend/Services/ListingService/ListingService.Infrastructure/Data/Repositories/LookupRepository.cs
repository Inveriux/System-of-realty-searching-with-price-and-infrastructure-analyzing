using ListingService.Application.Contracts.Repositories;
using ListingService.Domain.Abstractions;
using BasicBuildingBlocks.Repositories;
using BasicBuildingBlocks.Collections;
using Microsoft.EntityFrameworkCore;

namespace ListingService.Infrastructure.Data.Repositories;

public class LookupRepository<TLookup>(ApplicationDbContext context) : BaseRepository<TLookup>(context),
    ILookupRepository<TLookup>
    where TLookup : class, ILookupEntity
{
    public async Task<PagedResult<TLookup>> GetAllAsync<TKey>(PagedRequest pagedRequest,
         CancellationToken cancellationToken)
        => await GetAllAsync<TKey>(pagedRequest, null, false, false, cancellationToken);
    
    public async Task<List<TLookup>> GetAllAsync<TKey>(CancellationToken cancellationToken)
        => await GetAllAsync<TKey>(null, false, false, cancellationToken);

    public PagedResult<TLookup> GetAll<TKey>(PagedRequest pagedRequest)
        => GetAll<TKey>(pagedRequest, null, false, false);
    
    public List<TLookup> GetAll<TKey>()
        => GetAll<TKey>(null, false, false);

    public Task<TLookup?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => context.Set<TLookup>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name, cancellationToken);

    public async Task<bool> DeleteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var entity = await context.Set<TLookup>()
            .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name, cancellationToken);

        if (entity is null) 
            return false;

        context.Set<TLookup>().Remove(entity);  
        return true;
    }
}

using BasicBuildingBlocks.Extensions.Repositories;
using BasicBuildingBlocks.Pages;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BasicBuildingBlocks.Repositories;

public class BaseRepository<TEntity>(DbContext context) 
    where TEntity : class
{
    protected readonly DbContext _context = context;
    

    public async Task<PagedResult<TEntity>> GetAllAsync<TKey>(
        PagedRequest pageRequest,
        Expression<Func<TEntity, TKey>>? orderBy = null,
        bool isDescending = false,
        bool isTracking = false,
        CancellationToken ct = default)
    {
        var q = _context.Set<TEntity>().IsTracking(isTracking);

        orderBy ??= e => EF.Property<TKey>(e, "Id"); 
        q = isDescending ? q.OrderByDescending(orderBy) : q.OrderBy(orderBy);

        return await q.ToPagedResultAsync(pageRequest.PageNumber, pageRequest.PageSize, ct);
    }


    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        => await _context.Set<TEntity>().FindAsync([id], cancellationToken);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => (await _context.Set<TEntity>().AddAsync(entity, cancellationToken)).Entity;

    public TEntity Update(TEntity entity) => _context.Set<TEntity>().Update(entity).Entity;

    public void Delete(TEntity entity) => _context.Set<TEntity>().Remove(entity);

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default) 
        => await _context.SaveChangesAsync(cancellationToken) > 0;

    public bool SaveChanges() => _context.SaveChanges() > 0;
}
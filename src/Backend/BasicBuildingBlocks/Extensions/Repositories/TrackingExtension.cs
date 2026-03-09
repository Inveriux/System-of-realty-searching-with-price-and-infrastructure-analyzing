using Microsoft.EntityFrameworkCore;

namespace BasicBuildingBlocks.Extensions.Repositories;

public static class TrackingExtension
{
    public static IQueryable<TEntity> IsTracking<TEntity>(this IQueryable<TEntity> queryable, 
        bool isTracking = false)
        where TEntity : class
    {
        return isTracking ? queryable.AsTracking() : queryable.AsNoTracking();
    }
}
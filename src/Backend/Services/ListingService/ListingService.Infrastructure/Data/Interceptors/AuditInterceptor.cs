using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ListingService.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ListingService.Infrastructure.Data.Interceptors;

public class AuditInterceptor : ISaveChangesInterceptor
{
    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData evnetData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var context = evnetData.Context;

        if(context != null)
            UpdateEntries(context);

        return new ValueTask<InterceptionResult<int>>(result);
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, 
        InterceptionResult<int> result)
    {
        var context = eventData.Context;

        if(context != null)
            UpdateEntries(context);

        return result;
    }

    private void UpdateEntries(DbContext context)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added 
                || e.State == EntityState.Modified
                || HasChangedOwnedEntities(e));

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditableEntity entity)
            {
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.Created = now;
                }

                entity.LastModified = now;
            }
        }
    }

    private bool HasChangedOwnedEntities(EntityEntry entityEntry)
    {
        return entityEntry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
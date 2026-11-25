using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Data.Interceptors;

public sealed class AuditTimestampsInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyTimeStamps(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyTimeStamps(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTimeStamps(DbContext? context)
    {
        if(context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if(entry.State == EntityState.Added && entry.Property(entity => entity.CreatedAt).CurrentValue == default)
            {
                entry.Property(entity => entity.CreatedAt).CurrentValue = now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if(entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.UpdatedAt).CurrentValue = now;
            }
        }
    }
}

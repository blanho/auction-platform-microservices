using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Repository;

public abstract class EfRepository<TEntity, TContext> : IRepository<TEntity>, IBatchRepository<TEntity>
    where TEntity : AggregateRoot
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly IDateTimeProvider Clock;
    protected readonly IAuditContext AuditContext;

    protected EfRepository(TContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
    {
        Context = context;
        Clock = dateTime;
        AuditContext = auditContext;
    }

    protected DbSet<TEntity> Entities => Context.Set<TEntity>();

    protected virtual IQueryable<TEntity> Query => Entities.Where(entity => !entity.IsDeleted);

    public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public virtual Task<TEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query.AsTracking().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public virtual Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query.AnyAsync(entity => entity.Id == id, cancellationToken);

    public virtual Task<PaginatedResult<TEntity>> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default) =>
        Query.AsNoTracking()
            .OrderByDescending(entity => entity.CreatedAt)
            .ThenBy(entity => entity.Id)
            .ToPaginatedResultAsync(page, pageSize, cancellationToken);

    public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        CreateAsync(entity, cancellationToken);

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ApplyCreatedAudit(entity, Clock.UtcNowOffset);
        await Entities.AddAsync(entity, cancellationToken);
        return entity;
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        entity.SetUpdatedAudit(GetUpdatedBy(entity), Clock.UtcNowOffset);
        TrackUpdate(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await Query.AsTracking()
            .FirstOrDefaultAsync(entity => !entity.IsDeleted && entity.Id == id, cancellationToken);
        if (entity is not null)
            await DeleteAsync(entity, cancellationToken);
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!entity.IsDeleted)
            ApplyDeletedAudit(entity, Clock.UtcNowOffset);
        TrackUpdate(entity);
        return Task.CompletedTask;
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var batch = entities.ToList();
        var timestamp = Clock.UtcNowOffset;
        foreach (var entity in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ApplyCreatedAudit(entity, timestamp);
        }
        await Entities.AddRangeAsync(batch, cancellationToken);
    }

    public virtual Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var timestamp = Clock.UtcNowOffset;
        foreach (var entity in entities)
        {
            cancellationToken.ThrowIfCancellationRequested();
            entity.SetUpdatedAudit(GetUpdatedBy(entity), timestamp);
            TrackUpdate(entity);
        }
        return Task.CompletedTask;
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var keys = ids.Distinct().ToArray();
        if (keys.Length == 0)
            return;

        var batch = await Query.AsTracking()
            .Where(entity => !entity.IsDeleted && keys.Contains(entity.Id))
            .ToListAsync(cancellationToken);
        var timestamp = Clock.UtcNowOffset;
        foreach (var entity in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!entity.IsDeleted)
                ApplyDeletedAudit(entity, timestamp);
        }
    }

    protected virtual Guid GetCreatedBy(TEntity entity) => AuditContext.UserId;

    protected virtual Guid GetUpdatedBy(TEntity entity) => AuditContext.UserId;

    protected virtual Guid GetDeletedBy(TEntity entity) => AuditContext.UserId;

    protected virtual void ApplyCreatedAudit(TEntity entity, DateTimeOffset timestamp) =>
        entity.SetCreatedAudit(GetCreatedBy(entity), timestamp);

    protected virtual void ApplyDeletedAudit(TEntity entity, DateTimeOffset timestamp) =>
        entity.MarkAsDeleted(GetDeletedBy(entity), timestamp);

    protected virtual void TrackUpdate(TEntity entity)
    {
        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
            entry.State = EntityState.Modified;
    }
}

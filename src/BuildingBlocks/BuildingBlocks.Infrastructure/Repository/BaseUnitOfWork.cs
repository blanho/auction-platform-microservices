using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Infrastructure.Auditing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Repository;

public abstract class BaseUnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    protected readonly TContext Context;
    private readonly IMediator _mediator;
    private readonly IAuditPublisher? _auditPublisher;
    private readonly ChangeTrackerAuditCollector _auditCollector;

    protected BaseUnitOfWork(TContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
    {
        Context = context;
        _mediator = mediator;
        _auditPublisher = auditPublisher;
        _auditCollector = new ChangeTrackerAuditCollector();
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entities = Context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        _auditCollector.CollectAudits(Context);

        var result = await Context.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        if (_auditPublisher != null)
        {
            var pendingAudits = _auditCollector.GetPendingAudits();
            if (pendingAudits.Count > 0)
            {
                await _auditPublisher.PublishEntriesAsync(pendingAudits, cancellationToken);
            }
        }
        _auditCollector.Clear();

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = Context.Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = Context.Database.CurrentTransaction;
        if (transaction != null)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}

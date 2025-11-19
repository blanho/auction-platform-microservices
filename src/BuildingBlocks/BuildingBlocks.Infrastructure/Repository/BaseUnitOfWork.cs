using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Repository;

public abstract class BaseUnitOfWork<TContext> : IUnitOfWork, IDomainEventDispatcher
    where TContext : DbContext
{
    protected readonly TContext Context;
    private readonly IMediator _mediator;
    private readonly ILogger? _logger;
    private readonly List<IDomainEvent> _pendingDomainEvents = new();

    protected BaseUnitOfWork(TContext context, IMediator mediator, ILogger? logger = null)
    {
        Context = context;
        _mediator = mediator;
        _logger = logger;
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

        _pendingDomainEvents.AddRange(domainEvents);

        int result;

        var strategy = Context.Database.CreateExecutionStrategy();

        result = await strategy.ExecuteAsync(async () =>
        {

            var hasExistingTransaction = Context.Database.CurrentTransaction != null;

            if (!hasExistingTransaction)
            {
                await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var saveResult = await Context.SaveChangesAsync(cancellationToken);

                    await DispatchDomainEventsWithRetryAsync(domainEvents, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    _pendingDomainEvents.Clear();
                    return saveResult;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex,
                        "Transaction failed during SaveChanges. Rolling back. {EventCount} domain events were not dispatched.",
                        domainEvents.Count);
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            else
            {

                var saveResult = await Context.SaveChangesAsync(cancellationToken);
                await DispatchDomainEventsWithRetryAsync(domainEvents, cancellationToken);
                _pendingDomainEvents.Clear();
                return saveResult;
            }
        });

        return result;
    }

    private async Task DispatchDomainEventsWithRetryAsync(
        List<IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        var failedEvents = new List<(IDomainEvent Event, Exception Exception)>();

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex,
                    "Failed to dispatch domain event {EventType} on first attempt. Will retry.",
                    domainEvent.GetType().Name);
                failedEvents.Add((domainEvent, ex));
            }
        }

        foreach (var (failedEvent, originalEx) in failedEvents)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
                await _mediator.Publish(failedEvent, cancellationToken);
                _logger?.LogInformation(
                    "Successfully dispatched domain event {EventType} on retry.",
                    failedEvent.GetType().Name);
            }
            catch (Exception retryEx)
            {

                _logger?.LogError(retryEx,
                    "CRITICAL: Failed to dispatch domain event {EventType} after retry. " +
                    "Database changes are committed but event may be lost. " +
                    "Consider implementing outbox pattern recovery. Original error: {OriginalError}",
                    failedEvent.GetType().Name,
                    originalEx.Message);
            }
        }
    }

    [Obsolete("Prefer using SaveChangesAsync which handles event dispatch after commit automatically.")]
    public async Task DispatchEventsAsync(CancellationToken cancellationToken = default)
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

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
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

using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Repository;

public abstract class BaseUnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    protected readonly TContext Context;
    private readonly IMediator _mediator;

    protected BaseUnitOfWork(TContext context, IMediator mediator)
    {
        Context = context;
        _mediator = mediator;
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await PublishDomainEventsAsync(cancellationToken);
        try
        {
            return await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("A concurrency conflict occurred while saving changes.", ex);
        }
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        var entities = Context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = entities
            .SelectMany(entity => entity.DomainEvents)
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context.Dispose();
        }
    }
}

using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Events;
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

        int result;
        try
        {
            result = await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new BuildingBlocks.Domain.Exceptions.ConcurrencyException("A concurrency conflict occurred while saving changes.", ex);
        }

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

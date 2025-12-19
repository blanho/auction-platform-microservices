namespace Common.Repository.Interfaces;

public interface IBaseUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(CancellationToken cancellationToken = default);
}

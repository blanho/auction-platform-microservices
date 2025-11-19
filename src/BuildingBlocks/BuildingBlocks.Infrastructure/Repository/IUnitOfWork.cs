using AppIUnitOfWork = BuildingBlocks.Application.Abstractions.Persistence.IUnitOfWork;

namespace BuildingBlocks.Infrastructure.Repository;

public interface IUnitOfWork : AppIUnitOfWork
{
}

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(CancellationToken cancellationToken = default);
}

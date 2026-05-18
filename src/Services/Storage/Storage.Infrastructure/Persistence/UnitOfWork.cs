using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using StorageUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Storage.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<StorageDbContext>, StorageUnitOfWork
{
    public UnitOfWork(StorageDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

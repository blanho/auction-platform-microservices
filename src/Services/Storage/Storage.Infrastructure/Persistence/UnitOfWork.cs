using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Storage.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<StorageDbContext>
{
    public UnitOfWork(StorageDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

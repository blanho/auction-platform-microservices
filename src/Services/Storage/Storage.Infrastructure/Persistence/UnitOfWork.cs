using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using StorageUnitOfWork = Storage.Application.Interfaces.IUnitOfWork;

namespace Storage.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<StorageDbContext>, StorageUnitOfWork
{
    public UnitOfWork(StorageDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}

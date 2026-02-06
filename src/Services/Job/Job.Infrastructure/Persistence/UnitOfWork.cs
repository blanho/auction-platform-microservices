using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Infrastructure.Repository;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;
using MediatR;

namespace Jobs.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<JobDbContext>, IUnitOfWork
{
    public UnitOfWork(JobDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}

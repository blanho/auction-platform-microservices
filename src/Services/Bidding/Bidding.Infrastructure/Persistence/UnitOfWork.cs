using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Bidding.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<BidDbContext>, IUnitOfWork
{
    public UnitOfWork(BidDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}

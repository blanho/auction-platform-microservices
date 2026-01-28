using BuildingBlocks.Application.Abstractions.Auditing;
using MediatR;

namespace Bidding.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<BidDbContext>, IUnitOfWork
{
    public UnitOfWork(BidDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}

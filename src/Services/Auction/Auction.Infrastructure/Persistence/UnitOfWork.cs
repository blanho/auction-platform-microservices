using BuildingBlocks.Application.Abstractions.Auditing;
using MediatR;

namespace Auctions.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<AuctionDbContext>, IUnitOfWork
{
    public UnitOfWork(AuctionDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}


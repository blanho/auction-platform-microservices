using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Auctions.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<AuctionDbContext>, IUnitOfWork
{
    public UnitOfWork(AuctionDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}


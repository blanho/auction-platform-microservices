using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Auctions.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<AuctionDbContext>
{
    public UnitOfWork(AuctionDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}


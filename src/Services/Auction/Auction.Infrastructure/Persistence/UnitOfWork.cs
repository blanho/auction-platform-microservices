using MediatR;

namespace Auctions.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<AuctionDbContext>, IUnitOfWork
{
    public UnitOfWork(AuctionDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}


using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Bidding.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<BidDbContext>
{
    public UnitOfWork(BidDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

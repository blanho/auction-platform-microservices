

using MediatR;

namespace Bidding.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<BidDbContext>, IUnitOfWork
{
    public UnitOfWork(BidDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

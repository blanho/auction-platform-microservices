using AuctionService.Application.Interfaces;
using Common.Repository.Implementations;
using MediatR;

namespace AuctionService.Infrastructure.Data;

public class UnitOfWork : BaseUnitOfWork<AuctionDbContext>, IUnitOfWork
{
    public UnitOfWork(AuctionDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

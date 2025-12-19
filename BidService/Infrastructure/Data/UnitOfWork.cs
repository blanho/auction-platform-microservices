using BidService.Application.Interfaces;
using Common.Repository.Implementations;
using MediatR;

namespace BidService.Infrastructure.Data;

public class UnitOfWork : BaseUnitOfWork<BidDbContext>, IUnitOfWork
{
    public UnitOfWork(BidDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

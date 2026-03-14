using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using IUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Payment.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<PaymentDbContext>, IUnitOfWork
{
    public UnitOfWork(PaymentDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

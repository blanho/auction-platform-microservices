using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Payment.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<PaymentDbContext>
{
    public UnitOfWork(PaymentDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

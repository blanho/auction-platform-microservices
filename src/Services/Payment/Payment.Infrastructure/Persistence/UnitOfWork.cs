using MediatR;
using BuildingBlocks.Infrastructure.Repository;

namespace Payment.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<PaymentDbContext>
{
    public UnitOfWork(PaymentDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

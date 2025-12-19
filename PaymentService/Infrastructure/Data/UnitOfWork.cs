using Common.Repository.Implementations;
using MediatR;
using PaymentService.Application.Interfaces;

namespace PaymentService.Infrastructure.Data;

public class UnitOfWork : BaseUnitOfWork<PaymentDbContext>, IUnitOfWork
{
    public UnitOfWork(PaymentDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

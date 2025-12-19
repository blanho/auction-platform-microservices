using Common.Repository.Implementations;
using MediatR;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Data;

public class UnitOfWork : BaseUnitOfWork<NotificationDbContext>, IUnitOfWork
{
    public UnitOfWork(NotificationDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

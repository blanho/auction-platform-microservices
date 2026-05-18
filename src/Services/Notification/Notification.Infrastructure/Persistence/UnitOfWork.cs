using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using NotificationUnitOfWork = BuildingBlocks.Application.Abstractions.IUnitOfWork;

namespace Notification.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<NotificationDbContext>, NotificationUnitOfWork
{
    public UnitOfWork(NotificationDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

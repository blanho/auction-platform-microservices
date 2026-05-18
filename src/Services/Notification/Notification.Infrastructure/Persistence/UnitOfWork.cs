using BuildingBlocks.Infrastructure.Repository;
using MediatR;

namespace Notification.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<NotificationDbContext>
{
    public UnitOfWork(NotificationDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }
}

using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using NotificationUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Infrastructure.Persistence;

public class UnitOfWork : BaseUnitOfWork<NotificationDbContext>, NotificationUnitOfWork
{
    public UnitOfWork(NotificationDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }
}

using BuildingBlocks.Application.Abstractions.Auditing;
using Notification.Application.DTOs.Audit;
using Notification.Application.Interfaces;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Application.Features.Notifications.DeleteNotification;

public class DeleteNotificationCommandHandler : ICommandHandler<DeleteNotificationCommand>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;
    private readonly IAuditPublisher _auditPublisher;

    public DeleteNotificationCommandHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteNotificationCommandHandler> logger,
        IAuditPublisher auditPublisher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _auditPublisher = auditPublisher;
    }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting notification {NotificationId}", request.NotificationId);

        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
            return Result.Success();

        var oldNotificationData = NotificationAuditData.FromNotification(notification);

        await _repository.DeleteAsync(request.NotificationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditPublisher.PublishAsync(
            request.NotificationId,
            oldNotificationData,
            AuditAction.Deleted,
            oldNotificationData,
            cancellationToken: cancellationToken);

        return Result.Success();
    }
}

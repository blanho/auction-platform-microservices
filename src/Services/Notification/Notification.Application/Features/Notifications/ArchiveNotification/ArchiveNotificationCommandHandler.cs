using Notification.Application.Interfaces;

namespace Notification.Application.Features.Notifications.ArchiveNotification;

public class ArchiveNotificationCommandHandler : ICommandHandler<ArchiveNotificationCommand>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArchiveNotificationCommandHandler> _logger;

    public ArchiveNotificationCommandHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<ArchiveNotificationCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ArchiveNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Archiving notification {NotificationId}", request.NotificationId);

        await _repository.ArchiveAsync(request.NotificationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

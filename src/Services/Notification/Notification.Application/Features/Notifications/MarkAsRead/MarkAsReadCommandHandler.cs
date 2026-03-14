using Notification.Application.Interfaces;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Application.Features.Notifications.MarkAsRead;

public class MarkAsReadCommandHandler : ICommandHandler<MarkAsReadCommand>
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkAsReadCommandHandler> _logger;

    public MarkAsReadCommandHandler(
        INotificationRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<MarkAsReadCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Marking notification {NotificationId} as read", request.NotificationId);

        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
            return Result.Failure(Error.Create("Notification.NotFound", "Notification not found"));

        if (notification.UserId != request.UserId)
            return Result.Failure(Error.Create("Notification.Forbidden", "You do not own this notification"));

        await _repository.MarkAsReadAsync(request.NotificationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

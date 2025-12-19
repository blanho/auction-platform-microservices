using NotificationService.Application.Ports;

namespace NotificationService.Application.UseCases.DismissNotification;

public record DismissNotificationRequest
{
    public required Guid NotificationId { get; init; }
    public required string UserId { get; init; }
}

public record DismissNotificationResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public interface IDismissNotificationUseCase
{
    Task<DismissNotificationResponse> ExecuteAsync(
        DismissNotificationRequest request,
        CancellationToken cancellationToken = default);
}

public class DismissNotificationUseCase : IDismissNotificationUseCase
{
    private readonly INotificationRepository _repository;

    public DismissNotificationUseCase(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<DismissNotificationResponse> ExecuteAsync(
        DismissNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification == null)
        {
            return new DismissNotificationResponse
            {
                Success = false,
                ErrorMessage = "Notification not found"
            };
        }

        if (notification.UserId != request.UserId)
        {
            return new DismissNotificationResponse
            {
                Success = false,
                ErrorMessage = "Unauthorized"
            };
        }

        notification.Dismiss();
        await _repository.UpdateAsync(notification, cancellationToken);

        return new DismissNotificationResponse { Success = true };
    }
}

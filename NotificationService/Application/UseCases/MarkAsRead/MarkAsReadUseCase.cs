using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.UseCases.MarkAsRead;

public record MarkAsReadRequest
{
    public required Guid NotificationId { get; init; }
    public required string UserId { get; init; }
}

public record MarkAsReadResponse
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

public interface IMarkAsReadUseCase
{
    Task<MarkAsReadResponse> ExecuteAsync(
        MarkAsReadRequest request,
        CancellationToken cancellationToken = default);
}

public class MarkAsReadUseCase : IMarkAsReadUseCase
{
    private readonly INotificationRepository _repository;

    public MarkAsReadUseCase(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<MarkAsReadResponse> ExecuteAsync(
        MarkAsReadRequest request,
        CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(request.NotificationId, cancellationToken);

        if (notification == null)
        {
            return new MarkAsReadResponse
            {
                Success = false,
                ErrorMessage = "Notification not found"
            };
        }

        if (notification.UserId != request.UserId)
        {
            return new MarkAsReadResponse
            {
                Success = false,
                ErrorMessage = "Unauthorized"
            };
        }

        notification.MarkAsRead();
        await _repository.UpdateAsync(notification, cancellationToken);

        return new MarkAsReadResponse { Success = true };
    }
}

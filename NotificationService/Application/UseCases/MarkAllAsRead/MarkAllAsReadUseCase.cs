using NotificationService.Application.Ports;

namespace NotificationService.Application.UseCases.MarkAllAsRead;

public record MarkAllAsReadRequest
{
    public required string UserId { get; init; }
}

public record MarkAllAsReadResponse
{
    public bool Success { get; init; }
    public int UpdatedCount { get; init; }
}

public interface IMarkAllAsReadUseCase
{
    Task<MarkAllAsReadResponse> ExecuteAsync(
        MarkAllAsReadRequest request,
        CancellationToken cancellationToken = default);
}

public class MarkAllAsReadUseCase : IMarkAllAsReadUseCase
{
    private readonly INotificationRepository _repository;

    public MarkAllAsReadUseCase(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<MarkAllAsReadResponse> ExecuteAsync(
        MarkAllAsReadRequest request,
        CancellationToken cancellationToken = default)
    {
        var updatedCount = await _repository.MarkAllAsReadAsync(request.UserId, cancellationToken);

        return new MarkAllAsReadResponse
        {
            Success = true,
            UpdatedCount = updatedCount
        };
    }
}

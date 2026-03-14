namespace NotificationService.Contracts.Commands;

public record ProcessMarkAllNotificationsReadCommand
{
    public Guid CorrelationId { get; init; }
    public string UserId { get; init; } = string.Empty;
    public DateTimeOffset RequestedAt { get; init; }
}

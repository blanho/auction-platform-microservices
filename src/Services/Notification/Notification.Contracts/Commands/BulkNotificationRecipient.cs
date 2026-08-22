namespace NotificationService.Contracts.Commands;

public record BulkNotificationRecipient
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public Dictionary<string, string> Parameters { get; init; } = [];
}

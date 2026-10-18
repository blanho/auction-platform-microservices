namespace Notification.Domain.Entities;

public sealed record CreateUserNotificationParams
{
    public required string UserId { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? Link { get; init; }
    public Guid? AuctionId { get; init; }
    public Guid? BidId { get; init; }
    public Guid? OrderId { get; init; }
    public string? Type { get; init; }
}

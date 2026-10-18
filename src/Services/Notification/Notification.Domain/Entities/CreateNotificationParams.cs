using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

public sealed record CreateNotificationParams
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required NotificationType Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public ChannelType Channels { get; init; } = ChannelType.InApp;
    public string? Data { get; init; }
    public Guid? AuctionId { get; init; }
    public Guid? BidId { get; init; }
    public Guid? OrderId { get; init; }
    public string? ReferenceId { get; init; }
    public string? HtmlContent { get; init; }
}

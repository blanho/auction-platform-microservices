using Notification.Application.DTOs;

namespace Notification.Application.Interfaces;

public interface INotificationHubService
{

    Task SendNotificationToUserAsync(string userId, NotificationDto notification);

    Task SendNotificationToAllAsync(NotificationDto notification);

    Task SendBidPlacedToAuctionRoomAsync(string auctionId, object bidEvent);

    Task SendOutbidNotificationAsync(string userId, object outbidEvent);

    Task SendAuctionEndedToRoomAsync(string auctionId, object endedEvent);

    Task SendAuctionExtendedToRoomAsync(string auctionId, object extendedEvent);
}

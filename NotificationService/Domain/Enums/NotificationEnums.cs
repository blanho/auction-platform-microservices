namespace NotificationService.Domain.Enums
{
    public enum NotificationType
    {
        AuctionCreated,
        AuctionUpdated,
        AuctionDeleted,
        AuctionFinished,
        BidPlaced,
        BidAccepted,
        OutBid,
        AuctionWon,
        AuctionEndingSoon
    }

    public enum NotificationStatus
    {
        Unread,
        Read,
        Dismissed
    }
}

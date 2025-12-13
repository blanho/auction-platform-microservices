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
        AuctionEndingSoon,
        BuyNowExecuted,
        OrderCreated,
        OrderShipped,
        OrderDelivered,
        ReviewReceived,
        WelcomeEmail,
        PasswordReset
    }

    public enum NotificationStatus
    {
        Unread,
        Read,
        Dismissed
    }

    public enum NotificationChannel
    {
        InApp,
        Email,
        Both
    }
}

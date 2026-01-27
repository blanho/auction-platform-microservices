namespace Notification.Domain.Enums;

public enum NotificationType
{
    General = 0,

    AuctionCreated = 10,
    AuctionUpdated = 11,
    AuctionStarted = 12,
    AuctionEndingSoon = 13,
    AuctionFinished = 14,
    AuctionCancelled = 15,

    BidPlaced = 20,
    BidOutbid = 21,
    BidWon = 22,
    BidLost = 23,
    BidAccepted = 24,
    BidRejected = 25,

    PaymentReceived = 30,
    PaymentFailed = 31,
    PaymentRefunded = 32,

    WelcomeMessage = 40,
    AccountVerified = 41,
    PasswordChanged = 42,

    SystemAlert = 50,
    Maintenance = 51
}

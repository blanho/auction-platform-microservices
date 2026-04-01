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
    AuctionExtended = 16,
    AuctionImportCompleted = 17,
    AuctionExportCompleted = 18,
    BulkAuctionUpdateCompleted = 19,

    BidPlaced = 20,
    BidOutbid = 21,
    BidWon = 22,
    BidLost = 23,
    BidAccepted = 24,
    BidRejected = 25,
    AutoBidCreated = 26,
    AutoBidActivated = 27,
    AutoBidDeactivated = 28,
    AutoBidUpdated = 29,
    BidBelowReserve = 30,
    BidTooLow = 31,

    PaymentReceived = 40,
    PaymentFailed = 41,
    PaymentRefunded = 42,
    WalletCreated = 43,
    FundsDeposited = 44,
    FundsWithdrawn = 45,
    FundsHeld = 46,
    FundsReleased = 47,
    FundsDeducted = 48,
    OrderShipped = 49,
    OrderDelivered = 50,
    OrderReportReady = 51,

    WelcomeMessage = 60,
    AccountVerified = 61,
    PasswordChanged = 62,
    UserReactivated = 63,
    UserEmailConfirmed = 64,

    SystemAlert = 70,
    Maintenance = 71,

    JobCompleted = 80,
    JobFailed = 81
}

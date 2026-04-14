namespace BuildingBlocks.Application.Abstractions;

public static class ErrorCodes
{
    public static class General
    {
        public const string NotFound = "GENERAL.NOT_FOUND";
        public const string Conflict = "GENERAL.CONFLICT";
        public const string ValidationFailed = "GENERAL.VALIDATION_FAILED";
        public const string Unauthorized = "GENERAL.UNAUTHORIZED";
        public const string Forbidden = "GENERAL.FORBIDDEN";
        public const string InternalError = "GENERAL.INTERNAL_ERROR";
        public const string ServiceUnavailable = "GENERAL.SERVICE_UNAVAILABLE";
        public const string TooManyRequests = "GENERAL.TOO_MANY_REQUESTS";
        public const string ConcurrencyConflict = "GENERAL.CONCURRENCY_CONFLICT";
    }

    public static class Auction
    {
        public const string NotFound = "AUCTION.NOT_FOUND";
        public const string AlreadyStarted = "AUCTION.ALREADY_STARTED";
        public const string AlreadyFinished = "AUCTION.ALREADY_FINISHED";
        public const string InvalidStatus = "AUCTION.INVALID_STATUS";
        public const string ReservePriceNotMet = "AUCTION.RESERVE_PRICE_NOT_MET";
        public const string BuyNowNotAvailable = "AUCTION.BUY_NOW_NOT_AVAILABLE";
        public const string CannotModify = "AUCTION.CANNOT_MODIFY";
        public const string NotOwner = "AUCTION.NOT_OWNER";
        public const string InvalidDateRange = "AUCTION.INVALID_DATE_RANGE";
        public const string CategoryNotFound = "AUCTION.CATEGORY_NOT_FOUND";
        public const string BrandNotFound = "AUCTION.BRAND_NOT_FOUND";
    }

    public static class Bid
    {
        public const string NotFound = "BID.NOT_FOUND";
        public const string AuctionNotLive = "BID.AUCTION_NOT_LIVE";
        public const string BelowMinimum = "BID.BELOW_MINIMUM";
        public const string BelowIncrement = "BID.BELOW_INCREMENT";
        public const string OwnAuction = "BID.OWN_AUCTION";
        public const string AlreadyHighestBidder = "BID.ALREADY_HIGHEST_BIDDER";
        public const string LockFailed = "BID.LOCK_FAILED";
        public const string Rejected = "BID.REJECTED";
        public const string CannotRetract = "BID.CANNOT_RETRACT";
    }

    public static class Payment
    {
        public const string OrderNotFound = "PAYMENT.ORDER_NOT_FOUND";
        public const string InvalidOrderStatus = "PAYMENT.INVALID_ORDER_STATUS";
        public const string PaymentFailed = "PAYMENT.PAYMENT_FAILED";
        public const string RefundFailed = "PAYMENT.REFUND_FAILED";
        public const string InsufficientFunds = "PAYMENT.INSUFFICIENT_FUNDS";
        public const string WalletNotFound = "PAYMENT.WALLET_NOT_FOUND";
        public const string WalletAlreadyExists = "PAYMENT.WALLET_ALREADY_EXISTS";
        public const string InvalidAmount = "PAYMENT.INVALID_AMOUNT";
    }

    public static class Identity
    {
        public const string UserNotFound = "IDENTITY.USER_NOT_FOUND";
        public const string InvalidCredentials = "IDENTITY.INVALID_CREDENTIALS";
        public const string EmailNotConfirmed = "IDENTITY.EMAIL_NOT_CONFIRMED";
        public const string AccountLocked = "IDENTITY.ACCOUNT_LOCKED";
        public const string AccountSuspended = "IDENTITY.ACCOUNT_SUSPENDED";
        public const string TokenExpired = "IDENTITY.TOKEN_EXPIRED";
        public const string TwoFactorRequired = "IDENTITY.TWO_FACTOR_REQUIRED";
        public const string TwoFactorInvalid = "IDENTITY.TWO_FACTOR_INVALID";
        public const string EmailAlreadyExists = "IDENTITY.EMAIL_ALREADY_EXISTS";
    }

    public static class Notification
    {
        public const string NotFound = "NOTIFICATION.NOT_FOUND";
        public const string TemplateNotFound = "NOTIFICATION.TEMPLATE_NOT_FOUND";
        public const string DeliveryFailed = "NOTIFICATION.DELIVERY_FAILED";
        public const string InvalidChannel = "NOTIFICATION.INVALID_CHANNEL";
    }

    public static class Storage
    {
        public const string FileNotFound = "STORAGE.FILE_NOT_FOUND";
        public const string FileTooLarge = "STORAGE.FILE_TOO_LARGE";
        public const string InvalidFileType = "STORAGE.INVALID_FILE_TYPE";
        public const string UploadFailed = "STORAGE.UPLOAD_FAILED";
    }

    public static class Job
    {
        public const string NotFound = "JOB.NOT_FOUND";
        public const string AlreadyRunning = "JOB.ALREADY_RUNNING";
        public const string CannotCancel = "JOB.CANNOT_CANCEL";
        public const string CannotRetry = "JOB.CANNOT_RETRY";
    }
}

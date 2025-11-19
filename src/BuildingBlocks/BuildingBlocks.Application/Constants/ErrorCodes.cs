namespace BuildingBlocks.Application.Constants;

public static class ErrorCodes
{
    public static class Auction
    {
        public const string NotFound = "Auction.NotFound";
        public const string CreateFailed = "Auction.CreateFailed";
        public const string UpdateFailed = "Auction.UpdateFailed";
        public const string DeleteFailed = "Auction.DeleteFailed";
        public const string AlreadyFinished = "Auction.AlreadyFinished";
        public const string NotLive = "Auction.NotLive";
        public const string CannotModifyLive = "Auction.CannotModifyLive";
        public const string InvalidEndTime = "Auction.InvalidEndTime";
        public const string ReservePriceInvalid = "Auction.ReservePriceInvalid";
    }

    public static class Bid
    {
        public const string NotFound = "Bid.NotFound";
        public const string PlaceFailed = "Bid.PlaceFailed";
        public const string TooLow = "Bid.TooLow";
        public const string AuctionEnded = "Bid.AuctionEnded";
        public const string AuctionNotLive = "Bid.AuctionNotLive";
        public const string OwnAuction = "Bid.OwnAuction";
        public const string AlreadyProcessing = "Bid.AlreadyProcessing";
        public const string PreviouslyFailed = "Bid.PreviouslyFailed";
        public const string DuplicateDetected = "Bid.DuplicateDetected";
        public const string LockFailed = "Bid.LockFailed";
        public const string IncrementNotMet = "Bid.IncrementNotMet";
    }

    public static class BuyNow
    {
        public const string NotAvailable = "BuyNow.NotAvailable";
        public const string OwnAuction = "BuyNow.OwnAuction";
        public const string AuctionNotLive = "BuyNow.AuctionNotLive";
        public const string Conflict = "BuyNow.Conflict";
        public const string Failed = "BuyNow.Failed";
    }

    public static class Wallet
    {
        public const string NotFound = "Wallet.NotFound";
        public const string InsufficientBalance = "Wallet.InsufficientBalance";
        public const string Busy = "Wallet.Busy";
        public const string ConcurrencyConflict = "Wallet.ConcurrencyConflict";
        public const string InvalidAmount = "Wallet.InvalidAmount";
        public const string AlreadyExists = "Wallet.AlreadyExists";
        public const string Inactive = "Wallet.Inactive";
    }

    public static class Order
    {
        public const string NotFound = "Order.NotFound";
        public const string CreateFailed = "Order.CreateFailed";
        public const string InvalidStatus = "Order.InvalidStatus";
        public const string AlreadyPaid = "Order.AlreadyPaid";
        public const string PaymentFailed = "Order.PaymentFailed";
        public const string ShipmentFailed = "Order.ShipmentFailed";
    }

    public static class Payment
    {
        public const string Failed = "Payment.Failed";
        public const string DuplicateTransaction = "Payment.DuplicateTransaction";
        public const string RefundFailed = "Payment.RefundFailed";
        public const string InvalidAmount = "Payment.InvalidAmount";
    }

    public static class Authorization
    {
        public const string Unauthorized = "Auth.Unauthorized";
        public const string Forbidden = "Auth.Forbidden";
        public const string InvalidToken = "Auth.InvalidToken";
        public const string TokenExpired = "Auth.TokenExpired";
        public const string InsufficientPermissions = "Auth.InsufficientPermissions";
        public const string ResourceOwnershipRequired = "Auth.ResourceOwnershipRequired";
    }

    public static class Tenant
    {
        public const string NotFound = "Tenant.NotFound";
        public const string Invalid = "Tenant.Invalid";
        public const string AccessDenied = "Tenant.AccessDenied";
    }

    public static class Validation
    {
        public const string InvalidInput = "Validation.InvalidInput";
        public const string RequiredField = "Validation.RequiredField";
        public const string InvalidFormat = "Validation.InvalidFormat";
        public const string OutOfRange = "Validation.OutOfRange";
    }

    public static class System
    {
        public const string InternalError = "System.InternalError";
        public const string ServiceUnavailable = "System.ServiceUnavailable";
        public const string Timeout = "System.Timeout";
        public const string RateLimited = "System.RateLimited";
    }

    public static class Storage
    {
        public const string FileNotFound = "Storage.FileNotFound";
        public const string UploadFailed = "Storage.UploadFailed";
        public const string InvalidFileType = "Storage.InvalidFileType";
        public const string FileTooLarge = "Storage.FileTooLarge";
        public const string OwnerRequired = "Storage.OwnerRequired";
        public const string UploadExpired = "Storage.UploadExpired";
        public const string ConfirmationFailed = "Storage.ConfirmationFailed";
    }

    public static class Search
    {
        public const string IndexFailed = "Search.IndexFailed";
        public const string QueryFailed = "Search.QueryFailed";
        public const string TenantRequired = "Search.TenantRequired";
        public const string DocumentNotFound = "Search.DocumentNotFound";
    }

    public static class Notification
    {
        public const string SendFailed = "Notification.SendFailed";
        public const string TemplateNotFound = "Notification.TemplateNotFound";
        public const string InvalidRecipient = "Notification.InvalidRecipient";
        public const string PreferencesBlocked = "Notification.PreferencesBlocked";
    }

    public static class Analytics
    {
        public const string QueryFailed = "Analytics.QueryFailed";
        public const string AggregationFailed = "Analytics.AggregationFailed";
        public const string ReportNotFound = "Analytics.ReportNotFound";
    }
}

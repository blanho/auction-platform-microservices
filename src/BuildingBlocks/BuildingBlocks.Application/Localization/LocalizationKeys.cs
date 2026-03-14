namespace BuildingBlocks.Application.Localization;

public static class LocalizationKeys
{
    public static class Errors
    {
        public const string NotFound = "Error.NotFound";
        public const string Conflict = "Error.Conflict";
        public const string Unauthorized = "Error.Unauthorized";
        public const string Forbidden = "Error.Forbidden";
        public const string ValidationFailed = "Error.ValidationFailed";
        public const string InternalServerError = "Error.InternalServerError";
        public const string BadRequest = "Error.BadRequest";
        public const string ServiceUnavailable = "Error.ServiceUnavailable";
    }

    public static class Validation
    {
        public const string Required = "Validation.Required";
        public const string MaxLength = "Validation.MaxLength";
        public const string MinLength = "Validation.MinLength";
        public const string InvalidEmail = "Validation.InvalidEmail";
        public const string InvalidFormat = "Validation.InvalidFormat";
        public const string MustBePositive = "Validation.MustBePositive";
        public const string MustBeInRange = "Validation.MustBeInRange";
        public const string InvalidDate = "Validation.InvalidDate";
        public const string FutureDate = "Validation.FutureDate";
        public const string PastDate = "Validation.PastDate";
    }

    public static class Common
    {
        public const string Success = "Common.Success";
        public const string Failed = "Common.Failed";
        public const string Loading = "Common.Loading";
        public const string NoResults = "Common.NoResults";
        public const string OperationCompleted = "Common.OperationCompleted";
    }

    public static class Auction
    {
        public const string NotFound = "Auction.NotFound";
        public const string AlreadyEnded = "Auction.AlreadyEnded";
        public const string NotStarted = "Auction.NotStarted";
        public const string BidTooLow = "Auction.BidTooLow";
        public const string CannotBidOwnAuction = "Auction.CannotBidOwnAuction";
        public const string InvalidStatus = "Auction.InvalidStatus";
        public const string Created = "Auction.Created";
        public const string Updated = "Auction.Updated";
        public const string Deleted = "Auction.Deleted";
        public const string FetchFailed = "Auction.FetchFailed";
        public const string EndDatePassed = "Auction.EndDatePassed";
        public const string BulkUpdateFailed = "Auction.BulkUpdateFailed";
        public const string DeactivationFailed = "Auction.DeactivationFailed";
        public const string ActivationFailed = "Auction.ActivationFailed";
        public const string NotActive = "Auction.NotActive";
        public const string IndexFailed = "Auction.IndexFailed";
        public const string RemoveFailed = "Auction.RemoveFailed";
    }

    public static class BuyNow
    {
        public const string Conflict = "BuyNow.Conflict";
        public const string ConflictPurchased = "BuyNow.ConflictPurchased";
        public const string NotAvailable = "BuyNow.NotAvailable";
        public const string OwnAuction = "BuyNow.OwnAuction";
        public const string AuctionNotLive = "BuyNow.AuctionNotLive";
        public const string Failed = "BuyNow.Failed";
    }

    public static class Bookmark
    {
        public const string NotFound = "Bookmark.NotFound";
        public const string AlreadyExists = "Bookmark.AlreadyExists";
    }

    public static class Brand
    {
        public const string NotFound = "Brand.NotFound";
        public const string SlugExists = "Brand.SlugExists";
        public const string DeleteFailed = "Brand.DeleteFailed";
        public const string UpdateFailed = "Brand.UpdateFailed";
        public const string FetchError = "Brand.FetchError";
    }

    public static class Category
    {
        public const string NotFound = "Category.NotFound";
        public const string SlugExists = "Category.SlugExists";
        public const string SelfParent = "Category.SelfParent";
        public const string HasItems = "Category.HasItems";
        public const string CreateFailed = "Category.CreateFailed";
        public const string UpdateFailed = "Category.UpdateFailed";
        public const string DeleteFailed = "Category.DeleteFailed";
        public const string FetchError = "Categories.FetchError";
    }

    public static class Review
    {
        public const string NotFound = "Review.NotFound";
        public const string AlreadyExists = "Review.AlreadyExists";
        public const string InvalidRating = "Review.InvalidRating";
        public const string Forbidden = "Review.Forbidden";
        public const string AlreadyResponded = "Review.AlreadyResponded";
    }

    public static class Analytics
    {
        public const string Error = "Analytics.Error";
        public const string QueryFailed = "Analytics.QueryFailed";
        public const string InvalidDateRange = "Analytics.InvalidDateRange";
        public const string InvalidPeriod = "Analytics.InvalidPeriod";
    }

    public static class Dashboard
    {
        public const string Error = "Dashboard.Error";
    }

    public static class Auth
    {
        public const string UsernameExists = "Auth.UsernameExists";
        public const string EmailExists = "Auth.EmailExists";
        public const string InvalidCredentials = "Auth.InvalidCredentials";
        public const string InvalidPassword = "Auth.InvalidPassword";
        public const string AccountLocked = "Auth.AccountLocked";
        public const string AccountLockedOut = "Auth.AccountLockedOut";
        public const string AccountSuspended = "Auth.AccountSuspended";
        public const string AccountInactive = "Auth.AccountInactive";
        public const string EmailNotConfirmed = "Auth.EmailNotConfirmed";
        public const string EmailAlreadyConfirmed = "Auth.EmailAlreadyConfirmed";
        public const string InvalidConfirmationLink = "Auth.InvalidConfirmationLink";
        public const string ConfirmationFailed = "Auth.ConfirmationFailed";
        public const string InvalidResetRequest = "Auth.InvalidResetRequest";
        public const string ResetFailed = "Auth.ResetFailed";
        public const string RegistrationFailed = "Auth.RegistrationFailed";
        public const string InvalidRefreshToken = "Auth.InvalidRefreshToken";
        public const string SecurityTermination = "Auth.SecurityTermination";
        public const string SendEmailFailed = "Auth.SendEmailFailed";
        public const string InvalidAuthCode = "Auth.InvalidAuthCode";
        public const string AuthCodeExchangeFailed = "Auth.AuthCodeExchangeFailed";
    }

    public static class User
    {
        public const string NotFound = "User.NotFound";
        public const string AlreadyExists = "User.AlreadyExists";
        public const string InvalidCredentials = "User.InvalidCredentials";
        public const string AccountLocked = "User.AccountLocked";
        public const string EmailNotConfirmed = "User.EmailNotConfirmed";
        public const string PasswordResetSent = "User.PasswordResetSent";
        public const string PasswordChanged = "User.PasswordChanged";
        public const string ProfileUpdated = "User.ProfileUpdated";
        public const string UpdateFailed = "User.UpdateFailed";
        public const string DeleteFailed = "User.DeleteFailed";
        public const string SuspendFailed = "User.SuspendFailed";
        public const string UnsuspendFailed = "User.UnsuspendFailed";
        public const string ActivateFailed = "User.ActivateFailed";
        public const string DeactivateFailed = "User.DeactivateFailed";
        public const string RoleUpdateFailed = "User.RoleUpdateFailed";
        public const string CannotDeleteSelf = "User.CannotDeleteSelf";
        public const string AlreadySeller = "User.AlreadySeller";
        public const string AdminHasSellerPrivileges = "User.AdminHasSellerPrivileges";
        public const string SellerUpgradeFailed = "User.SellerUpgradeFailed";
        public const string InvalidToken = "User.InvalidToken";
    }

    public static class TwoFactor
    {
        public const string NotEnabled = "TwoFactor.NotEnabled";
        public const string AlreadyEnabled = "TwoFactor.AlreadyEnabled";
        public const string InvalidCode = "TwoFactor.InvalidCode";
        public const string InvalidRecoveryCode = "TwoFactor.InvalidRecoveryCode";
        public const string InvalidPassword = "TwoFactor.InvalidPassword";
        public const string DisableFailed = "TwoFactor.DisableFailed";
        public const string SetupFailed = "TwoFactor.SetupFailed";
        public const string AccountLocked = "TwoFactor.AccountLocked";
    }

    public static class Profile
    {
        public const string UpdateFailed = "Profile.UpdateFailed";
        public const string AvatarUploadFailed = "Profile.AvatarUploadFailed";
        public const string PasswordChangeFailed = "Profile.PasswordChangeFailed";
        public const string InvalidCurrentPassword = "Profile.InvalidCurrentPassword";
    }

    public static class External
    {
        public const string ProviderError = "External.ProviderError";
        public const string EmailNotProvided = "External.EmailNotProvided";
        public const string LinkFailed = "External.LinkFailed";
        public const string UnlinkFailed = "External.UnlinkFailed";
        public const string CannotUnlinkLastLogin = "External.CannotUnlinkLastLogin";
        public const string ProcessFailed = "External.ProcessFailed";
    }

    public static class Bid
    {
        public const string NotFound = "Bid.NotFound";
        public const string Unauthorized = "Bid.Unauthorized";
        public const string AlreadyRejected = "Bid.AlreadyRejected";
        public const string RetractWindowExpired = "Bid.RetractWindowExpired";
        public const string PlaceFailed = "Bid.PlaceFailed";
        public const string TooLow = "Bid.TooLow";
        public const string AuctionNotLive = "Bid.AuctionNotLive";
        public const string SelfBidding = "Bid.SelfBidding";
        public const string DuplicateBid = "Bid.DuplicateBid";
    }

    public static class AutoBid
    {
        public const string NotFound = "AutoBid.NotFound";
        public const string Unauthorized = "AutoBid.Unauthorized";
        public const string AlreadyExists = "AutoBid.AlreadyExists";
        public const string AlreadyCancelled = "AutoBid.AlreadyCancelled";
        public const string Inactive = "AutoBid.Inactive";
        public const string MaxAmountTooLow = "AutoBid.MaxAmountTooLow";
        public const string UpdateFailed = "AutoBid.UpdateFailed";
        public const string CreateFailed = "AutoBid.CreateFailed";
    }

    public static class Order
    {
        public const string NotFound = "Order.NotFound";
        public const string AlreadyExists = "Order.AlreadyExists";
        public const string AlreadyPaid = "Order.AlreadyPaid";
        public const string Cancelled = "Order.Cancelled";
        public const string InvalidStatus = "Order.InvalidStatus";
        public const string InvalidData = "Order.InvalidData";
        public const string NotPaid = "Order.NotPaid";
        public const string AlreadyShipped = "Order.AlreadyShipped";
        public const string AlreadyDelivered = "Order.AlreadyDelivered";
        public const string NotShipped = "Order.NotShipped";
        public const string CreateFailed = "Order.CreateFailed";
        public const string UpdateFailed = "Order.UpdateFailed";
    }

    public static class Wallet
    {
        public const string NotFound = "Wallet.NotFound";
        public const string AlreadyExists = "Wallet.AlreadyExists";
        public const string Busy = "Wallet.Busy";
        public const string InsufficientBalance = "Wallet.InsufficientBalance";
        public const string InsufficientHeldAmount = "Wallet.InsufficientHeldAmount";
        public const string ConcurrencyConflict = "Wallet.ConcurrencyConflict";
        public const string CreateFailed = "Wallet.CreateFailed";
    }

    public static class Payment
    {
        public const string Failed = "Payment.Failed";
        public const string Succeeded = "Payment.Succeeded";
        public const string Pending = "Payment.Pending";
        public const string Refunded = "Payment.Refunded";
        public const string InsufficientFunds = "Payment.InsufficientFunds";
        public const string ProcessingFailed = "Payment.ProcessingFailed";
        public const string RefundFailed = "Payment.RefundFailed";
        public const string InvalidAmount = "Payment.InvalidAmount";
    }

    public static class Stripe
    {
        public const string SessionCreationFailed = "Stripe.SessionCreationFailed";
        public const string WebhookProcessingFailed = "Stripe.WebhookProcessingFailed";
        public const string InvalidWebhookSignature = "Stripe.InvalidWebhookSignature";
    }

    public static class Transaction
    {
        public const string NotFound = "Transaction.NotFound";
        public const string Failed = "Transaction.Failed";
    }

    public static class Notification
    {
        public const string NotFound = "Notification.NotFound";
        public const string Unauthorized = "Notification.Unauthorized";
        public const string SendFailed = "Notification.SendFailed";
        public const string MarkReadFailed = "Notification.MarkReadFailed";
        public const string JoinForbidden = "Notification.JoinForbidden";
        public const string LeaveForbidden = "Notification.LeaveForbidden";
    }

    public static class Template
    {
        public const string NotFound = "Template.NotFound";
        public const string KeyExists = "Template.KeyExists";
        public const string CreateFailed = "Template.CreateFailed";
        public const string UpdateFailed = "Template.UpdateFailed";
        public const string DeleteFailed = "Template.DeleteFailed";
    }

    public static class Preference
    {
        public const string NotFound = "Preference.NotFound";
        public const string UpdateFailed = "Preference.UpdateFailed";
    }

    public static class Email
    {
        public const string SendFailed = "Email.SendFailed";
        public const string InvalidRecipient = "Email.InvalidRecipient";
    }

    public static class Search
    {
        public const string Failed = "Search.Failed";
        public const string InvalidQuery = "Search.InvalidQuery";
        public const string IndexNotReady = "Search.IndexNotReady";
    }

    public static class Index
    {
        public const string CreateFailed = "Index.CreateFailed";
        public const string RebuildFailed = "Index.RebuildFailed";
        public const string HealthCheckFailed = "Index.HealthCheckFailed";
        public const string NotFound = "Index.NotFound";
    }

    public static class IndexError
    {
        public const string IndexingFailed = "IndexError.IndexingFailed";
        public const string DocumentNotFound = "IndexError.DocumentNotFound";
        public const string UpdateFailed = "IndexError.UpdateFailed";
        public const string DeleteFailed = "IndexError.DeleteFailed";
        public const string BulkIndexFailed = "IndexError.BulkIndexFailed";
        public const string ConnectionFailed = "IndexError.ConnectionFailed";
        public const string IndexCreationFailed = "IndexError.IndexCreationFailed";
    }

    public static class Suggestion
    {
        public const string Failed = "Suggestion.Failed";
    }

    public static class Storage
    {
        public const string FileNotFound = "Storage.FileNotFound";
        public const string EmptyFile = "Storage.EmptyFile";
        public const string FileTooLarge = "Storage.FileTooLarge";
        public const string InvalidExtension = "Storage.InvalidExtension";
        public const string InvalidContentType = "Storage.InvalidContentType";
        public const string NoFiles = "Storage.NoFiles";
        public const string TooManyFiles = "Storage.TooManyFiles";
        public const string DeleteFailed = "Storage.DeleteFailed";
        public const string PresignedUrlNotSupported = "Storage.PresignedUrlNotSupported";
    }

    public static class Report
    {
        public const string NotFound = "Report.NotFound";
        public const string AlreadyResolved = "Report.AlreadyResolved";
        public const string CannotDeleteResolved = "Report.CannotDeleteResolved";
        public const string InvalidStatus = "Report.InvalidStatus";
        public const string CreateFailed = "Report.CreateFailed";
        public const string UpdateFailed = "Report.UpdateFailed";
        public const string DeleteFailed = "Report.DeleteFailed";
    }

    public static class Setting
    {
        public const string NotFound = "Setting.NotFound";
        public const string KeyExists = "Setting.KeyExists";
        public const string SystemSettingReadOnly = "Setting.SystemSettingReadOnly";
        public const string InvalidValue = "Setting.InvalidValue";
        public const string CreateFailed = "Setting.CreateFailed";
        public const string UpdateFailed = "Setting.UpdateFailed";
        public const string DeleteFailed = "Setting.DeleteFailed";
    }

    public static class AuditLog
    {
        public const string NotFound = "AuditLog.NotFound";
        public const string QueryFailed = "AuditLog.QueryFailed";
    }

    public static class Job
    {
        public const string NotFound = "Job.NotFound";
        public const string Duplicate = "Job.Duplicate";
        public const string InvalidType = "Job.InvalidType";
        public const string CannotCancel = "Job.CannotCancel";
        public const string CannotRetry = "Job.CannotRetry";
        public const string StartFailed = "Job.StartFailed";
        public const string ProcessingFailed = "Job.ProcessingFailed";
    }

    public static class JobItem
    {
        public const string NotFound = "JobItem.NotFound";
        public const string AlreadyProcessed = "JobItem.AlreadyProcessed";
        public const string ProcessingFailed = "JobItem.ProcessingFailed";
    }

    public static class RateLimit
    {
        public const string TooManyRequests = "RateLimit.TooManyRequests";
        public const string RetryAfter = "RateLimit.RetryAfter";
    }
}

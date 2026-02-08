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
    }

    public static class Payment
    {
        public const string Failed = "Payment.Failed";
        public const string Succeeded = "Payment.Succeeded";
        public const string Pending = "Payment.Pending";
        public const string Refunded = "Payment.Refunded";
        public const string InsufficientFunds = "Payment.InsufficientFunds";
    }

    public static class Common
    {
        public const string Success = "Common.Success";
        public const string Failed = "Common.Failed";
        public const string Loading = "Common.Loading";
        public const string NoResults = "Common.NoResults";
        public const string OperationCompleted = "Common.OperationCompleted";
    }
}

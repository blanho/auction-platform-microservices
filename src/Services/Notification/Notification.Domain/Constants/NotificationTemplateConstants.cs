namespace Notification.Domain.Constants;

public static class NotificationTemplateKeys
{
    public const string Welcome = "welcome";
    public const string EmailConfirmation = "email-confirmation";
    public const string PasswordChanged = "password-changed";
    public const string LoginNotification = "login-notification";
    public const string TwoFactorEnabled = "2fa-enabled";
    public const string TwoFactorDisabled = "2fa-disabled";
    public const string SecurityAlertGeneral = "security-alert-general";
    public const string SecurityAlertTokenTheft = "security-alert-token-theft";
    public const string SecurityAlertSuspiciousLogin = "security-alert-suspicious-login";
    public const string SecurityAlertFailedLogins = "security-alert-failed-logins";
    public const string SecurityAlertSessionsRevoked = "security-alert-sessions-revoked";
}

public static class NotificationTemplateDataKeys
{
    public const string Username = "username";
    public const string FullName = "fullName";
    public const string ConfirmationLink = "confirmationLink";
    public const string ChangedAt = "changedAt";
    public const string IpAddress = "ipAddress";
    public const string LoginAt = "loginAt";
    public const string AlertType = "alertType";
    public const string Description = "description";
    public const string OccurredAt = "occurredAt";
    public const string EnabledAt = "enabledAt";
    public const string DisabledAt = "disabledAt";
}

public static class NotificationPayloadDataKeys
{
    public const string AuctionId = "AuctionId";
    public const string BidId = "BidId";
    public const string Amount = "Amount";
    public const string Reason = "Reason";
    public const string PreviousBidAmount = "PreviousBidAmount";
    public const string NewHighBidAmount = "NewHighBidAmount";
}

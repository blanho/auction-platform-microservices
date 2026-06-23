namespace Identity.Domain.Constants;

public static class IdentityDefaults
{
    public static class Token
    {
        public const int AccessTokenExpirationMinutes = 15;
        public const int AccessTokenExpirationSeconds = AccessTokenExpirationMinutes * 60;
        public const int RefreshTokenExpirationDays = 7;
        public const int RefreshTokenAbsoluteExpirationDays = 30;
    }

    public static class OAuth
    {
        public const string DefaultClientId = "nextApp";
    }

    public static class EmailTemplate
    {
        public const string UsernameKey = "username";
        public const string ConfirmationLinkKey = "confirmationLink";
        public const string ResetLinkKey = "resetLink";
    }

    public static class Audit
    {
        public const string TwoFactorEnabled = "2fa_enabled";
        public const string TwoFactorDisabled = "2fa_disabled";
        public const string PasswordChange = "password_change";
        public const string ProfileUpdate = "profile_update";
        public const string Login = "login";
        public const string Suspend = "suspend";
        public const string Unsuspend = "unsuspend";
        public const string Activate = "activate";
        public const string Deactivate = "deactivate";
        public const string RoleChange = "role_change";
    }
}

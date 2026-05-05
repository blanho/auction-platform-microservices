namespace Identity.Api.Constants;

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
}

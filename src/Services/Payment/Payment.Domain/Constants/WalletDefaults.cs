namespace Payment.Domain.Constants;

public static class WalletDefaults
{
    public static class Lock
    {
        public static readonly TimeSpan StandardExpiry = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan ExtendedExpiry = TimeSpan.FromSeconds(30);
    }

    public const string DefaultCurrency = "USD";
}

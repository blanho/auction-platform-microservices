using BuildingBlocks.Domain.Constants;

namespace Payment.Domain.Constants;

public static class WalletDefaults
{
    public const string DefaultCurrency = CurrencyCodes.Usd;

    public static class Lock
    {
        public static readonly TimeSpan StandardExpiry = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan ExtendedExpiry = TimeSpan.FromSeconds(30);

        public const string WalletOperationKeyFormat = "wallet:operation:{0}";

        public static string GetWalletOperationKey(string username) =>
            string.Format(WalletOperationKeyFormat, username);
    }

    public static class Audit
    {
        public const string Deposit = "Deposit";
        public const string Withdraw = "Withdraw";
        public const string HoldFunds = "HoldFunds";
        public const string ReleaseFunds = "ReleaseFunds";
        public const string ProcessWalletPayment = "ProcessWalletPayment";
    }

    public static class Messaging
    {
        public const int OutboxQueryDelaySeconds = 10;

        public const int ConnectionTimeoutSeconds = 30;

        public const int ContinuationTimeoutSeconds = 20;

        public const int StandardRetryLimit = 3;

        public const int HighThroughputRetryLimit = 5;

        public const int StandardMinIntervalSeconds = 1;

        public const int HighThroughputMinIntervalMs = 200;

        public const int MaxIntervalSeconds = 30;

        public const int StandardIntervalDeltaSeconds = 5;

        public const int ReportIntervalDeltaSeconds = 2;

        public const int PrefetchCountReports = 4;

        public const int RedeliveryFastSeconds = 5;

        public const int RedeliverySlowSeconds = 30;

        public const int RedeliveryMaxMinutes = 2;
    }

    public static class Persistence
    {
        public const int MoneyPrecision = 18;

        public const int MoneyScale = 2;

        public const int UsernameMaxLength = 200;

        public const int ItemTitleMaxLength = 500;

        public const int AddressMaxLength = 1000;

        public const int TrackingNumberMaxLength = 100;

        public const int CarrierMaxLength = 100;

        public const int NotesMaxLength = 1000;

        public const int TransactionIdMaxLength = 200;

        public const int DescriptionMaxLength = 500;

        public const int ReferenceTypeMaxLength = 100;

        public const int PaymentMethodMaxLength = 100;

        public const int ExternalTransactionIdMaxLength = 200;
    }
}

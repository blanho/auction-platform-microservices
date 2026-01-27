namespace Payment.Application.Helpers;

public static class TransactionReferenceHelper
{
    private const string ReferencePrefix = "TXN";

    public static string GenerateTransactionReference()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"{ReferencePrefix}-{timestamp}-{random}";
    }

    public static string GenerateOrderReference(Guid auctionId)
    {
        var shortAuctionId = auctionId.ToString("N")[..8].ToUpperInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        return $"ORD-{timestamp}-{shortAuctionId}";
    }

    public static string GenerateRefundReference(string originalReference)
    {
        return $"REF-{originalReference}";
    }

    public static string GenerateDepositReference(Guid userId)
    {
        var shortUserId = userId.ToString("N")[..8].ToUpperInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        return $"DEP-{timestamp}-{shortUserId}";
    }
}

namespace BidService.Domain.ValueObjects;

public static class BidIncrement
{
    private static readonly List<BidIncrementRule> IncrementRules = new()
    {
        new BidIncrementRule(0, 99, 5),
        new BidIncrementRule(100, 499, 10),
        new BidIncrementRule(500, 999, 25),
        new BidIncrementRule(1000, 4999, 50),
        new BidIncrementRule(5000, 9999, 100),
        new BidIncrementRule(10000, 24999, 250),
        new BidIncrementRule(25000, 49999, 500),
        new BidIncrementRule(50000, 99999, 1000),
        new BidIncrementRule(100000, 249999, 2500),
        new BidIncrementRule(250000, 499999, 5000),
        new BidIncrementRule(500000, int.MaxValue, 10000)
    };

    public static int GetMinimumIncrement(int currentBid)
    {
        var rule = IncrementRules.FirstOrDefault(r => currentBid >= r.MinBid && currentBid <= r.MaxBid);
        return rule?.Increment ?? 10;
    }

    public static int GetMinimumNextBid(int currentBid)
    {
        return currentBid + GetMinimumIncrement(currentBid);
    }

    public static bool IsValidBidAmount(int bidAmount, int currentHighBid)
    {
        if (currentHighBid == 0)
        {
            return bidAmount > 0;
        }

        var minimumNextBid = GetMinimumNextBid(currentHighBid);
        return bidAmount >= minimumNextBid;
    }

    public static string GetIncrementErrorMessage(int bidAmount, int currentHighBid)
    {
        var minimumIncrement = GetMinimumIncrement(currentHighBid);
        var minimumNextBid = GetMinimumNextBid(currentHighBid);
        return $"Bid must be at least ${minimumNextBid:N0}. Minimum increment is ${minimumIncrement:N0} for bids at this level.";
    }
}

public record BidIncrementRule(int MinBid, int MaxBid, int Increment);

namespace Bidding.Domain.ValueObjects;

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
        new BidIncrementRule(500000, decimal.MaxValue, 10000)
    };

    public static decimal GetIncrement(decimal currentBid)
    {
        var rule = IncrementRules.FirstOrDefault(r => currentBid >= r.MinBid && currentBid <= r.MaxBid);
        return rule?.Increment ?? 10;
    }

    public static decimal GetMinimumIncrement(decimal currentBid)
    {
        return GetIncrement(currentBid);
    }

    public static decimal GetMinimumNextBid(decimal currentBid)
    {
        return currentBid + GetIncrement(currentBid);
    }

    public static bool IsValidBidAmount(decimal bidAmount, decimal currentHighBid)
    {
        if (currentHighBid == 0)
        {
            return bidAmount > 0;
        }

        var minimumNextBid = GetMinimumNextBid(currentHighBid);
        return bidAmount >= minimumNextBid;
    }

    public static string GetIncrementErrorMessage(decimal bidAmount, decimal currentHighBid)
    {
        var minimumIncrement = GetIncrement(currentHighBid);
        var minimumNextBid = GetMinimumNextBid(currentHighBid);
        return $"Bid must be at least ${minimumNextBid:N2}. Minimum increment is ${minimumIncrement:N2} for bids at this level.";
    }
}

public record BidIncrementRule(decimal MinBid, decimal MaxBid, decimal Increment);

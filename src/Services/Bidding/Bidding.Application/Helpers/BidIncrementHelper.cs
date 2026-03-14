namespace Bidding.Application.Helpers;

public static class BidIncrementHelper
{
    public static decimal GetIncrement(decimal currentBid)
    {
        return currentBid switch
        {
            < 100 => 5,
            < 500 => 10,
            < 1000 => 25,
            < 5000 => 50,
            < 10000 => 100,
            < 25000 => 250,
            < 50000 => 500,
            < 100000 => 1000,
            < 250000 => 2500,
            < 500000 => 5000,
            _ => 10000
        };
    }

    public static decimal GetMinimumNextBid(decimal currentHighBid)
    {
        if (currentHighBid <= 0)
            return 0;

        return currentHighBid + GetIncrement(currentHighBid);
    }

    public static bool IsValidBidAmount(decimal bidAmount, decimal currentHighBid)
    {
        if (currentHighBid <= 0)
            return bidAmount > 0;

        return bidAmount >= GetMinimumNextBid(currentHighBid);
    }
}

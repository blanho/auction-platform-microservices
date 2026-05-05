namespace Bidding.Application.Services;

public static class BidLockKeys
{
    public static string ForAuction(Guid auctionId) => $"auction-bid:{auctionId}";
}

namespace Bidding.Application.Services;

public static class AutoBidLockKeys
{
    public static string ForAuction(Guid auctionId) => $"autobid:auction:{auctionId}";
}

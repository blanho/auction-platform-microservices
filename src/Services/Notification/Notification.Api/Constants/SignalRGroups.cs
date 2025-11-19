namespace Notification.Api.Constants;

public static class SignalRGroups
{
    private const string AuctionRoomPrefix = "auction";

    public static string ForAuctionRoom(string auctionId)
        => $"{AuctionRoomPrefix}-{auctionId}";

    public static string ForAuctionRoom(Guid auctionId)
        => ForAuctionRoom(auctionId.ToString());
}

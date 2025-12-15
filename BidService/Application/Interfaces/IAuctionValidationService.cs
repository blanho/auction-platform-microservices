namespace BidService.Application.Interfaces;

public interface IAuctionValidationService
{
    Task<AuctionValidationResult> ValidateAuctionForBidAsync(
        Guid auctionId,
        string bidder,
        int bidAmount,
        CancellationToken cancellationToken = default);

    Task<AuctionInfo?> GetAuctionDetailsAsync(
        Guid auctionId,
        CancellationToken cancellationToken = default);

    Task<bool> ExtendAuctionAsync(
        Guid auctionId,
        int extendMinutes,
        string reason,
        CancellationToken cancellationToken = default);
}

public class AuctionValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int CurrentHighBid { get; set; }
    public int ReservePrice { get; set; }
    public DateTimeOffset? AuctionEnd { get; set; }
    public string? Seller { get; set; }
    public string? Status { get; set; }
}

public class AuctionInfo
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Seller { get; set; } = string.Empty;
    public string? Winner { get; set; }
    public int CurrentHighBid { get; set; }
    public int ReservePrice { get; set; }
    public int? BuyNowPrice { get; set; }
    public DateTimeOffset AuctionEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsBuyNowAvailable { get; set; }
}

namespace Bidding.Application.Features.Bids.RetractBid;

public record RetractBidCommand(
    Guid BidId,
    Guid UserId,
    string Reason) : ICommand<RetractBidResult>;

public record RetractBidResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? BidId { get; init; }
    public decimal? RefundAmount { get; init; }

    public static RetractBidResult Succeeded(Guid bidId, decimal refundAmount) => new()
    {
        Success = true,
        BidId = bidId,
        RefundAmount = refundAmount
    };

    public static RetractBidResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}

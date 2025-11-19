namespace Bidding.Application.Features.AutoBids.CreateAutoBid;

public record CreateAutoBidCommand(
    Guid AuctionId,
    Guid UserId,
    string Username,
    decimal MaxAmount,
    decimal? IncrementAmount = null) : ICommand<CreateAutoBidResult>;

public record CreateAutoBidResult
{
    public bool Success { get; init; }
    public Guid? AutoBidId { get; init; }
    public string? ErrorMessage { get; init; }
    public AutoBidDto? AutoBid { get; init; }

    public static CreateAutoBidResult Succeeded(AutoBidDto autoBid) => new()
    {
        Success = true,
        AutoBidId = autoBid.Id,
        AutoBid = autoBid
    };

    public static CreateAutoBidResult Failed(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}

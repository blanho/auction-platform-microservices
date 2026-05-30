using BuildingBlocks.Application.Abstractions;

namespace Bidding.Application.Features.Bids.PlaceBid;

public static class BidErrors
{
    public static Error BidTooLow(string reason) => Error.Create(
        "Bid.TooLow",
        reason);

    public static Error Rejected(string reason) => Error.Create(
        "Bid.Rejected",
        reason);

    public static readonly Error DuplicateRequest = Error.Create(
        "Bid.DuplicateRequest",
        "This bid request was already processed. Use a new idempotency key for a new bid.");
}

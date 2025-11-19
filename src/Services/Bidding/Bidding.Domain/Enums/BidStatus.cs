namespace Bidding.Domain.Enums;

public enum BidStatus
{
    Pending,
    Accepted,
    AcceptedBelowReserve,
    TooLow,
    Rejected
}

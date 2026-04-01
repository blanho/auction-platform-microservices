namespace Auctions.Domain.Enums;

public enum Status
{
    Draft,
    Scheduled,
    Live,
    Finished,
    ReservedNotMet,
    Inactive,
    Cancelled,
    ReservedForBuyNow
}

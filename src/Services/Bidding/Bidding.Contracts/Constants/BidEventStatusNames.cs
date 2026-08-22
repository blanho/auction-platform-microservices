namespace BidService.Contracts.Constants;

public enum BidEventStatus
{
    Placed,
    Accepted,
    AcceptedBelowReserve
}

public static class BidEventStatusNames
{
    public const string Placed = nameof(BidEventStatus.Placed);
    public const string Accepted = nameof(BidEventStatus.Accepted);
    public const string AcceptedBelowReserve = nameof(BidEventStatus.AcceptedBelowReserve);
}

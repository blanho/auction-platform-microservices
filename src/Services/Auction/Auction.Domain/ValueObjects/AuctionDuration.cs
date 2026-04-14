using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace Auctions.Domain.ValueObjects;

public sealed class AuctionDuration : ValueObject
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    private static readonly TimeSpan MinimumDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaximumDuration = TimeSpan.FromDays(30);

    private AuctionDuration(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    public static AuctionDuration Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new DomainInvariantException("Auction end must be after start.");

        var duration = end - start;

        if (duration < MinimumDuration)
            throw new DomainInvariantException(
                $"Auction duration must be at least {MinimumDuration.TotalHours} hours.");

        if (duration > MaximumDuration)
            throw new DomainInvariantException(
                $"Auction duration cannot exceed {MaximumDuration.TotalDays} days.");

        return new AuctionDuration(start, end);
    }

    public static AuctionDuration CreateFromEnd(DateTimeOffset end)
    {
        return Create(DateTimeOffset.UtcNow, end);
    }

    public TimeSpan Duration => End - Start;
    public bool IsActive => DateTimeOffset.UtcNow >= Start && DateTimeOffset.UtcNow < End;
    public bool HasEnded => DateTimeOffset.UtcNow >= End;
    public bool HasStarted => DateTimeOffset.UtcNow >= Start;
    public TimeSpan TimeRemaining => HasEnded ? TimeSpan.Zero : End - DateTimeOffset.UtcNow;

    public bool IsEndingSoon(TimeSpan threshold) =>
        IsActive && TimeRemaining <= threshold;

    public AuctionDuration ExtendBy(TimeSpan extension)
    {
        if (extension <= TimeSpan.Zero)
            throw new DomainInvariantException("Extension must be positive.");

        return new AuctionDuration(Start, End.Add(extension));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() =>
        $"{Start:yyyy-MM-dd HH:mm} → {End:yyyy-MM-dd HH:mm} ({Duration.TotalHours:F1}h)";
}

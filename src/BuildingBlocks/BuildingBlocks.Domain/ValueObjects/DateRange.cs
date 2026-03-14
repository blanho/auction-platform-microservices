using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Exceptions;

namespace BuildingBlocks.Domain.ValueObjects;

public sealed class DateRange : ValueObject, IComparable<DateRange>
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public DateRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new DomainInvariantException("DateRange end must be after start.");

        Start = start;
        End = end;
    }

    public static DateRange Create(DateTimeOffset start, DateTimeOffset end) => new(start, end);

    public static DateRange FromDuration(DateTimeOffset start, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new DomainInvariantException("Duration must be positive.");

        return new DateRange(start, start.Add(duration));
    }

    public TimeSpan Duration => End - Start;

    public double TotalDays => Duration.TotalDays;

    public double TotalHours => Duration.TotalHours;

    public bool Contains(DateTimeOffset point) => point >= Start && point <= End;

    public bool Contains(DateRange other) => other.Start >= Start && other.End <= End;

    public bool Overlaps(DateRange other) => Start < other.End && End > other.Start;

    public bool IsExpired(DateTimeOffset now) => now > End;

    public bool HasStarted(DateTimeOffset now) => now >= Start;

    public bool IsActive(DateTimeOffset now) => HasStarted(now) && !IsExpired(now);

    public DateRange ExtendBy(TimeSpan extension)
    {
        if (extension <= TimeSpan.Zero)
            throw new DomainInvariantException("Extension must be a positive duration.");

        return new DateRange(Start, End.Add(extension));
    }

    public DateRange ShortenBy(TimeSpan reduction)
    {
        if (reduction <= TimeSpan.Zero)
            throw new DomainInvariantException("Reduction must be a positive duration.");

        var newEnd = End.Subtract(reduction);
        if (newEnd <= Start)
            throw new DomainInvariantException("Reduction would make end before or equal to start.");

        return new DateRange(Start, newEnd);
    }

    public DateRange? Intersection(DateRange other)
    {
        if (!Overlaps(other))
            return null;

        var intersectionStart = Start > other.Start ? Start : other.Start;
        var intersectionEnd = End < other.End ? End : other.End;

        return new DateRange(intersectionStart, intersectionEnd);
    }

    public int CompareTo(DateRange? other)
    {
        if (other is null) return 1;
        var startComparison = Start.CompareTo(other.Start);
        return startComparison != 0 ? startComparison : End.CompareTo(other.End);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }

    public override string ToString() => $"{Start:yyyy-MM-dd HH:mm} → {End:yyyy-MM-dd HH:mm}";
}

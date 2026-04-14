namespace BuildingBlocks.Domain.ValueObjects;

#pragma warning disable S1210
public abstract class StronglyTypedId<T> : IEquatable<StronglyTypedId<T>>, IComparable<StronglyTypedId<T>>
#pragma warning restore S1210
    where T : StronglyTypedId<T>
{
    public Guid Value { get; }

    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
            throw new Exceptions.DomainInvariantException($"{typeof(T).Name} cannot be empty.");
        Value = value;
    }

    public static implicit operator Guid(StronglyTypedId<T> id) => id.Value;

    public bool Equals(StronglyTypedId<T>? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        obj is StronglyTypedId<T> other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(StronglyTypedId<T>? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString();

    public static bool operator ==(StronglyTypedId<T>? left, StronglyTypedId<T>? right) =>
        Equals(left, right);

    public static bool operator !=(StronglyTypedId<T>? left, StronglyTypedId<T>? right) =>
        !Equals(left, right);

    public static bool operator <(StronglyTypedId<T>? left, StronglyTypedId<T>? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator >(StronglyTypedId<T>? left, StronglyTypedId<T>? right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator <=(StronglyTypedId<T>? left, StronglyTypedId<T>? right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >=(StronglyTypedId<T>? left, StronglyTypedId<T>? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;
}

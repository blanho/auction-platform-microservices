using System.Reflection;

namespace BuildingBlocks.Domain.Entities;

#pragma warning disable S1210
public abstract class Enumeration : IComparable
#pragma warning restore S1210
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public static IEnumerable<T> GetAll<T>() where T : Enumeration =>
        typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();

    public static T FromValue<T>(int value) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(e => e.Id == value)
            ?? throw new Exceptions.DomainInvariantException(
                $"'{value}' is not a valid value for {typeof(T).Name}.");

    public static T FromName<T>(string name) where T : Enumeration =>
        GetAll<T>().FirstOrDefault(e =>
            string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
            ?? throw new Exceptions.DomainInvariantException(
                $"'{name}' is not a valid name for {typeof(T).Name}.");

    public static bool TryFromValue<T>(int value, out T? result) where T : Enumeration
    {
        result = GetAll<T>().FirstOrDefault(e => e.Id == value);
        return result is not null;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration other)
            return false;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? obj) =>
        obj is Enumeration enumeration ? Id.CompareTo(enumeration.Id) : -1;
}

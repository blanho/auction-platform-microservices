namespace BuildingBlocks.Domain.Guards;

public static class Guard
{
    public static Guid AgainstEmpty(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
            throw new Exceptions.DomainInvariantException($"{parameterName} cannot be empty.");
        return value;
    }

    public static string AgainstNullOrEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.DomainInvariantException($"{parameterName} is required.");
        return value;
    }

    public static string AgainstOverflow(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
            throw new Exceptions.DomainInvariantException(
                $"{parameterName} must not exceed {maxLength} characters.");
        return value;
    }

    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new Exceptions.DomainInvariantException($"{parameterName} cannot be null.");
        return value;
    }

    public static T AgainstNull<T>(T? value, string parameterName) where T : struct
    {
        if (!value.HasValue)
            throw new Exceptions.DomainInvariantException($"{parameterName} cannot be null.");
        return value.Value;
    }

    public static decimal AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
            throw new Exceptions.DomainInvariantException($"{parameterName} cannot be negative.");
        return value;
    }

    public static int AgainstNegative(int value, string parameterName)
    {
        if (value < 0)
            throw new Exceptions.DomainInvariantException($"{parameterName} cannot be negative.");
        return value;
    }

    public static decimal AgainstNonPositive(decimal value, string parameterName)
    {
        if (value <= 0)
            throw new Exceptions.DomainInvariantException($"{parameterName} must be positive.");
        return value;
    }

    public static int AgainstOutOfRange(int value, int min, int max, string parameterName)
    {
        if (value < min || value > max)
            throw new Exceptions.DomainInvariantException(
                $"{parameterName} must be between {min} and {max}.");
        return value;
    }

    public static decimal AgainstOutOfRange(decimal value, decimal min, decimal max, string parameterName)
    {
        if (value < min || value > max)
            throw new Exceptions.DomainInvariantException(
                $"{parameterName} must be between {min} and {max}.");
        return value;
    }

    public static DateTimeOffset AgainstPast(DateTimeOffset value, string parameterName)
    {
        if (value <= DateTimeOffset.UtcNow)
            throw new Exceptions.DomainInvariantException($"{parameterName} must be in the future.");
        return value;
    }

    public static IReadOnlyCollection<T> AgainstEmptyCollection<T>(
        IReadOnlyCollection<T>? collection, string parameterName)
    {
        if (collection is null || collection.Count == 0)
            throw new Exceptions.DomainInvariantException(
                $"{parameterName} must contain at least one element.");
        return collection;
    }
}

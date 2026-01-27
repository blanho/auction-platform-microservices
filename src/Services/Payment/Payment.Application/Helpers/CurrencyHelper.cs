namespace Payment.Application.Helpers;

public static class CurrencyHelper
{
    private static readonly Dictionary<string, int> DecimalPlaces = new(StringComparer.OrdinalIgnoreCase)
    {
        { "USD", 2 },
        { "EUR", 2 },
        { "GBP", 2 },
        { "VND", 0 },
        { "JPY", 0 },
        { "KRW", 0 }
    };

    public static decimal RoundToMinorUnit(decimal amount, string currencyCode)
    {
        var decimals = GetDecimalPlaces(currencyCode);
        return Math.Round(amount, decimals, MidpointRounding.AwayFromZero);
    }

    public static int GetDecimalPlaces(string currencyCode)
    {
        return DecimalPlaces.TryGetValue(currencyCode, out var places) ? places : 2;
    }

    public static long ToMinorUnits(decimal amount, string currencyCode)
    {
        var decimals = GetDecimalPlaces(currencyCode);
        var multiplier = (decimal)Math.Pow(10, decimals);
        return (long)(RoundToMinorUnit(amount, currencyCode) * multiplier);
    }

    public static decimal FromMinorUnits(long minorUnits, string currencyCode)
    {
        var decimals = GetDecimalPlaces(currencyCode);
        var divisor = (decimal)Math.Pow(10, decimals);
        return minorUnits / divisor;
    }

    public static bool IsValidCurrency(string currencyCode)
    {
        return DecimalPlaces.ContainsKey(currencyCode);
    }
}

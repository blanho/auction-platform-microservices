namespace Auctions.Application.Helpers;

public static class PriceHelper
{
    public static string FormatPrice(decimal price, string currencyCode = "USD")
    {
        return currencyCode.ToUpperInvariant() switch
        {
            "USD" => $"${price:N2}",
            "EUR" => $"€{price:N2}",
            "GBP" => $"£{price:N2}",
            "VND" => $"₫{price:N0}",
            "JPY" => $"¥{price:N0}",
            _ => $"{price:N2} {currencyCode}"
        };
    }

    public static decimal CalculateBuyerPremium(decimal winningBid, decimal premiumPercentage)
    {
        return Math.Round(winningBid * (premiumPercentage / 100), 2);
    }

    public static decimal CalculateTotalPrice(decimal winningBid, decimal premiumPercentage)
    {
        return winningBid + CalculateBuyerPremium(winningBid, premiumPercentage);
    }

    public static bool IsValidReservePrice(decimal reservePrice, decimal startingPrice)
    {
        return reservePrice >= startingPrice;
    }
}

using BuildingBlocks.Domain.Constants;

namespace Auctions.Application.Helpers;

public static class PriceHelper
{
    public static string FormatPrice(decimal price, string currencyCode = CurrencyCodes.Usd)
    {
        return currencyCode.ToUpperInvariant() switch
        {
            CurrencyCodes.Usd => $"${price:N2}",
            CurrencyCodes.Eur => $"€{price:N2}",
            CurrencyCodes.Gbp => $"£{price:N2}",
            CurrencyCodes.Vnd => $"₫{price:N0}",
            CurrencyCodes.Jpy => $"¥{price:N0}",
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

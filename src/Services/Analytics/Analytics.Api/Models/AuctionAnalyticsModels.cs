namespace Analytics.Api.Models;

public record AuctionSummary(
    int TotalAuctions,
    int AuctionsSold,
    int AuctionsUnsold,
    decimal SellThroughRate,
    decimal TotalGMV,
    decimal AverageSellingPrice
);

public record AuctionsByDate(
    DateOnly Date,
    int AuctionsCreated,
    int AuctionsFinished,
    int AuctionsSold,
    decimal Volume
);

public record TopSeller(
    Guid SellerId,
    string Username,
    int AuctionsSold,
    decimal TotalRevenue,
    decimal AveragePrice
);

public record SellerDashboard(
    int TotalAuctions,
    int ActiveAuctions,
    int SoldAuctions,
    decimal TotalRevenue,
    decimal AveragePrice,
    decimal SellThroughRate,
    int TotalBidsReceived
);

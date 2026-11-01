using Auctions.Domain.Entities;

namespace Auctions.Application.Features.Auctions.ExportAuctions;

public static class ExportAuctionMapper
{
    public static List<ExportAuctionRow> ToRows(IEnumerable<Auction> auctions) =>
        auctions.Select(auction => new ExportAuctionRow(
            AuctionId: auction.Id,
            Title: auction.Item.Title,
            Seller: auction.SellerUsername,
            Status: auction.Status.ToString(),
            Currency: auction.Currency,
            ReservePrice: auction.ReservePrice,
            CurrentHighBid: auction.CurrentHighBid,
            SoldAmount: auction.SoldAmount,
            CreatedAt: auction.CreatedAt,
            AuctionEnd: auction.AuctionEnd,
            Category: auction.Item.CategoryName,
            Condition: auction.Item.Condition)).ToList();
}

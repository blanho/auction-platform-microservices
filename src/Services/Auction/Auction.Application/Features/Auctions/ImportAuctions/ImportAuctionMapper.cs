using AuctionService.Contracts.Commands;
using Auctions.Domain.Entities;

namespace Auctions.Application.Features.Auctions.ImportAuctions;

public static class ImportAuctionMapper
{
    public static ImportAuctionRow ToRow(ImportAuctionItemPayload payload) => new(
        Title: payload.Title,
        Description: payload.Description,
        Condition: payload.Condition,
        YearManufactured: payload.YearManufactured,
        ReservePrice: payload.ReservePrice,
        BuyNowPrice: payload.BuyNowPrice,
        AuctionEnd: payload.AuctionEnd,
        CategoryId: payload.CategoryId,
        BrandId: payload.BrandId,
        Attributes: payload.Attributes);

    public static Auction ToAuction(
        ImportAuctionRow row,
        Guid sellerId,
        string sellerUsername,
        string currency,
        ISanitizationService sanitizationService)
    {
        var item = Item.Create(new CreateItemParams
        {
            Title = sanitizationService.SanitizeText(row.Title),
            Description = sanitizationService.SanitizeHtml(row.Description),
            Condition = row.Condition,
            YearManufactured = row.YearManufactured,
            CategoryId = row.CategoryId,
            BrandId = row.BrandId
        });

        if (row.Attributes is not null)
        {
            foreach (var attribute in row.Attributes)
            {
                item.SetAttribute(attribute.Key, attribute.Value);
            }
        }

        return Auction.Create(new CreateAuctionParams(
            SellerId: sellerId,
            SellerUsername: sellerUsername,
            Item: item,
            ReservePrice: row.ReservePrice,
            AuctionEnd: row.AuctionEnd,
            Currency: currency,
            BuyNowPrice: row.BuyNowPrice,
            IsFeatured: false));
    }
}

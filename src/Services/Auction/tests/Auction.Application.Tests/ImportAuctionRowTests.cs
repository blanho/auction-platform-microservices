using AuctionService.Contracts.Commands;
using Auctions.Application.Features.Auctions.ImportAuctions;
using BuildingBlocks.Application.Abstractions;
using Xunit;

namespace Auction.Application.Tests;

public class ImportAuctionRowTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void InvalidRowReportsEveryFieldWithItsOriginalRowNumber()
    {
        var row = new ImportAuctionRow("", "", null, 1899, -1, -2, Now);

        var errors = ImportAuctionRowValidator.Validate(row, 73, Now);

        Assert.Equal(new[] { "Title", "Description", "ReservePrice", "BuyNowPrice", "AuctionEnd", "YearManufactured" },
            errors.Select(error => error.Field));
        Assert.All(errors, error => Assert.Equal(73, error.RowNumber));
    }

    [Theory]
    [InlineData(1900, true)]
    [InlineData(2027, true)]
    [InlineData(1899, false)]
    [InlineData(2028, false)]
    public void ManufacturingYearIncludesBothBoundaries(int year, bool valid)
    {
        var errors = ImportAuctionRowValidator.Validate(ValidRow() with { YearManufactured = year }, 1, Now);
        Assert.Equal(valid, errors.Count == 0);
        if (!valid)
        {
            Assert.Equal("YearManufactured", Assert.Single(errors).Field);
        }
    }

    [Fact]
    public void AuctionEndMustBeStrictlyInTheFuture()
    {
        Assert.Equal("AuctionEnd", Assert.Single(ImportAuctionRowValidator.Validate(
            ValidRow() with { AuctionEnd = Now }, 1, Now)).Field);
        Assert.Empty(ImportAuctionRowValidator.Validate(ValidRow() with { AuctionEnd = Now.AddTicks(1) }, 1, Now));
    }

    [Fact]
    public void BuyNowMustExceedReserveButIsOptional()
    {
        Assert.Equal("BuyNowPrice", Assert.Single(ImportAuctionRowValidator.Validate(
            ValidRow() with { BuyNowPrice = 10 }, 1, Now)).Field);
        Assert.Empty(ImportAuctionRowValidator.Validate(ValidRow() with { BuyNowPrice = null }, 1, Now));
    }

    [Fact]
    public void PayloadMappingPreservesCatalogAttributesAndSanitizesContent()
    {
        var payload = new ImportAuctionItemPayload
        {
            RowNumber = 41,
            Title = "raw title",
            Description = "raw description",
            Condition = "Used",
            YearManufactured = 2020,
            ReservePrice = 10,
            BuyNowPrice = 25,
            AuctionEnd = Now.AddDays(2),
            CategoryId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Attributes = new Dictionary<string, string> { ["color"] = "blue" }
        };
        var sanitizer = InterfaceStub.For<ISanitizationService>((method, args) => method.Name switch
        {
            nameof(ISanitizationService.SanitizeText) => $"text:{args[0]}",
            nameof(ISanitizationService.SanitizeHtml) => $"html:{args[0]}",
            _ => throw new NotSupportedException(method.Name)
        });
        var sellerId = Guid.NewGuid();

        var auction = ImportAuctionMapper.ToAuction(ImportAuctionMapper.ToRow(payload), sellerId, "seller", "EUR", sanitizer);

        Assert.Equal("text:raw title", auction.Item.Title);
        Assert.Equal("html:raw description", auction.Item.Description);
        Assert.Equal(payload.Condition, auction.Item.Condition);
        Assert.Equal(payload.YearManufactured, auction.Item.YearManufactured);
        Assert.Equal(payload.CategoryId, auction.Item.CategoryId);
        Assert.Equal(payload.BrandId, auction.Item.BrandId);
        Assert.Equal("blue", auction.Item.Attributes["color"]);
        Assert.Equal(payload.ReservePrice, auction.ReservePrice);
        Assert.Equal(payload.BuyNowPrice, auction.BuyNowPrice);
        Assert.Equal(payload.AuctionEnd, auction.AuctionEnd);
        Assert.Equal(sellerId, auction.SellerId);
        Assert.Equal("seller", auction.SellerUsername);
        Assert.Equal("EUR", auction.Currency);
        Assert.False(auction.IsFeatured);
    }

    private static ImportAuctionRow ValidRow() => new("Title", "Description", null, null, 10, 20, Now.AddDays(1));
}

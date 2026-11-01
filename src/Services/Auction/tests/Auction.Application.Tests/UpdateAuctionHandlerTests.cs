using Auctions.Application.DTOs;
using Auctions.Application.Errors;
using Auctions.Application.Features.Auctions.UpdateAuction;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Events;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Application.Tests;

public class UpdateAuctionHandlerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdatePreservesSanitizationCatalogValidationAndAuditBeforeSave(bool missingBrand)
    {
        var auction = AuctionEntity.Create(new CreateAuctionParams(
            Guid.NewGuid(), "seller", Item.Create("old title", "old description"), 10,
            DateTimeOffset.UtcNow.AddDays(1), BuyNowPrice: 100));
        var request = new UpdateAuctionCommand(
            Id: auction.Id, Title: "new title", Description: "new description", Condition: "Used",
            YearManufactured: 2020, Attributes: new Dictionary<string, string> { ["color"] = "blue" },
            ReservePrice: 20, BuyNowPrice: 200, CategoryId: Guid.NewGuid(), BrandId: Guid.NewGuid(),
            IsFeatured: true, AuctionEnd: auction.AuctionEnd.AddDays(1), UserId: auction.SellerId);
        var calls = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var repository = InterfaceStub.For<IAuctionWriteRepository>((method, args) =>
        {
            Assert.Equal(token, args.OfType<CancellationToken>().Single());
            if (method.Name == nameof(IAuctionWriteRepository.GetByIdForUpdateAsync))
            {
                return Task.FromResult<AuctionEntity?>(auction);
            }
            Assert.Equal(nameof(IAuctionWriteRepository.UpdateAsync), method.Name);
            calls.Add("update");
            return Task.CompletedTask;
        });
        var catalog = InterfaceStub.For<ICatalogGrpcClient>((method, args) =>
        {
            Assert.Equal(token, args.OfType<CancellationToken>().Single());
            var category = method.Name == nameof(ICatalogGrpcClient.GetCategoryAsync);
            calls.Add(category ? "category" : "brand");
            Assert.Equal(category ? request.CategoryId : request.BrandId, args[0]);
            var name = category ? "Category" : "Brand";
            return Task.FromResult<(string Name, string Slug)?>(!category && missingBrand ? null : (name, "slug"));
        });
        var sanitizer = InterfaceStub.For<ISanitizationService>((_, args) => $"clean:{args[0]}");
        string[]? auditedFields = null;
        var audit = InterfaceStub.For<IAuditPublisher>((_, args) =>
        {
            Assert.Equal(token, args.OfType<CancellationToken>().Single());
            Assert.Equal("old title", Assert.IsType<AuctionAuditData>(args[3]).Title);
            Assert.Equal("clean:new title", Assert.IsType<AuctionAuditData>(args[1]).Title);
            var metadata = Assert.IsType<Dictionary<string, object>>(args[4]);
            auditedFields = Assert.IsAssignableFrom<IEnumerable<string>>(metadata[AuditMetadataKeys.ModifiedFields]).ToArray();
            calls.Add("audit");
            return Task.CompletedTask;
        });
        var unit = InterfaceStub.For<IUnitOfWork>((method, args) =>
        {
            Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
            Assert.Equal(token, args.OfType<CancellationToken>().Single());
            calls.Add("save");
            return Task.FromResult(1);
        });
        var handler = new UpdateAuctionCommandHandler(repository, NullLogger<UpdateAuctionCommandHandler>.Instance,
            unit, sanitizer, audit, catalog);

        var result = await handler.Handle(request, token);

        Assert.Equal(!missingBrand, result.IsSuccess);
        if (missingBrand)
        {
            Assert.Equal(AuctionErrors.Brand.NotFoundById(request.BrandId!.Value).Code, result.Error!.Code);
            Assert.Equal(new[] { "category", "brand" }, calls);
            Assert.Empty(auction.DomainEvents.OfType<AuctionUpdatedDomainEvent>());
            return;
        }

        Assert.Equal(new[] { "category", "brand", "update", "audit", "save" }, calls);
        Assert.Equal("clean:new description", auction.Item.Description);
        Assert.Equal("Used", auction.Item.Condition);
        Assert.Equal(2020, auction.Item.YearManufactured);
        Assert.Equal("blue", auction.Item.Attributes["color"]);
        Assert.Equal("Category", auction.Item.CategoryName);
        Assert.Equal("Brand", auction.Item.BrandName);
        Assert.Equal(20, auction.ReservePrice);
        Assert.Equal(200, auction.BuyNowPrice);
        Assert.True(auction.IsFeatured);
        Assert.Equal(request.AuctionEnd, auction.AuctionEnd);
        Assert.Equal(new[] { "Title", "Description", "Condition", "YearManufactured", "Attributes",
            "ReservePrice", "BuyNowPrice", "CategoryId", "BrandId", "IsFeatured", "AuctionEnd" }, auditedFields);
        Assert.Equal(auditedFields, Assert.Single(auction.DomainEvents.OfType<AuctionUpdatedDomainEvent>()).ModifiedFields);
    }
}

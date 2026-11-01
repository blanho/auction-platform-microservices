using System.Globalization;
using AuctionService.Contracts.Commands;
using Auctions.Application.Features.Auctions.QueueAuctionImport;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Localization;
using JobService.Contracts.Commands;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auction.Application.Tests;

public class QueuedImportBatchingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(1001)]
    public async Task QueuePreservesBatchBoundariesRowOrderAndPublishesBeforeSave(int rowCount)
    {
        using var culture = new RequestCultureScope("ja-JP");
        using var cancellation = new CancellationTokenSource();
        var messages = new List<object>();
        var calls = new List<string>();
        var publisher = InterfaceStub.For<IEventPublisher>((_, args) =>
        {
            Assert.Equal(cancellation.Token, args.OfType<CancellationToken>().Single());
            messages.Add(args[0]!);
            calls.Add("publish");
            return Task.CompletedTask;
        });
        var unit = InterfaceStub.For<IUnitOfWork>((_, args) =>
        {
            Assert.Equal(cancellation.Token, args.OfType<CancellationToken>().Single());
            calls.Add("save");
            return Task.FromResult(1);
        });
        var rows = Enumerable.Range(1, rowCount).Select(number => new QueueImportAuctionRow(
            $"Item {number}", "Description", "Used", 2020, 10, 20, DateTimeOffset.UtcNow.AddDays(1),
            Guid.NewGuid(), Guid.NewGuid(), new Dictionary<string, string> { ["color"] = "blue" })).ToList();
        var request = new QueueAuctionImportCommand(Guid.NewGuid(), "seller", "EUR", rows);
        var handler = new QueueAuctionImportCommandHandler(unit, publisher,
            NullLogger<QueueAuctionImportCommandHandler>.Instance);

        var result = await handler.Handle(request, cancellation.Token);

        Assert.True(result.IsSuccess);
        var job = Assert.IsType<RequestJobCommand>(messages[0]);
        Assert.Equal(rowCount, job.TotalItems);
        Assert.Equal(result.Value!.CorrelationId, job.CorrelationId);
        var batches = messages.OfType<ProcessAuctionImportBatchCommand>().ToList();
        var expectedBatches = (rowCount + 999) / 1000;
        Assert.Equal(expectedBatches, batches.Count);
        Assert.Equal(Enumerable.Range(1, expectedBatches), batches.Select(batch => batch.BatchNumber));
        Assert.All(batches, batch =>
        {
            Assert.Equal(expectedBatches, batch.TotalBatches);
            Assert.Equal(rowCount, batch.TotalRows);
            Assert.Equal(request.SellerId, batch.SellerId);
            Assert.Equal(request.SellerUsername, batch.SellerUsername);
            Assert.Equal(request.Currency, batch.Currency);
            Assert.Equal(CultureInfo.CurrentUICulture.Name, batch.Culture);
            Assert.Equal(result.Value.JobId, batch.CorrelationId);
            Assert.InRange(batch.Rows.Count, 1, 1000);
        });
        var payloads = batches.SelectMany(batch => batch.Rows).ToList();
        Assert.Equal(Enumerable.Range(1, rowCount), payloads.Select(row => row.RowNumber));
        foreach (var (row, payload) in rows.Zip(payloads))
        {
            Assert.Equal(row.Title, payload.Title);
            Assert.Equal(row.CategoryId, payload.CategoryId);
            Assert.Equal(row.BrandId, payload.BrandId);
            Assert.Equal(row.Attributes, payload.Attributes);
        }
        Assert.Equal("save", calls[^1]);
        Assert.Equal(messages.Count + 1, calls.Count);
    }
}

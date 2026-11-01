using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Interfaces;
using Auctions.Infrastructure.Messaging.Consumers;
using BuildingBlocks.Application.Abstractions;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class ImportConsumerProcessingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ImportSavesValidRowsReportsOriginalErrorsAndIgnoresCompletedReplay(bool batched)
    {
        var correlationId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var rows = new List<ImportAuctionItemPayload>
        {
            new() { RowNumber = 17, Title = "item", Description = "description", ReservePrice = 10,
                AuctionEnd = DateTimeOffset.UtcNow.AddDays(1), CategoryId = Guid.NewGuid() },
            new() { RowNumber = 88, AuctionEnd = DateTimeOffset.UtcNow.AddDays(-1) }
        };
        var writes = new List<string>();
        var published = new List<object>();
        var workflow = new TestWorkflowStore();
        var bulk = TestProxy.Create<IAuctionBulkRepository>((method, args) =>
        {
            Assert.Equal(nameof(IAuctionBulkRepository.BulkInsertAsync), method.Name);
            var auction = Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<AuctionEntity>>(args![0]));
            Assert.Equal(sellerId, auction.SellerId);
            Assert.Equal("seller", auction.SellerUsername);
            Assert.Equal("EUR", auction.Currency);
            Assert.Equal("item", auction.Item.Title);
            Assert.Equal(rows[0].CategoryId, auction.Item.CategoryId);
            writes.Add("insert");
            return Task.CompletedTask;
        });
        var unit = TestProxy.Create<IUnitOfWork>((method, _) =>
        {
            Assert.Equal(nameof(IUnitOfWork.SaveChangesAsync), method.Name);
            writes.Add("save");
            return Task.FromResult(1);
        });
        var sanitizer = TestProxy.Create<ISanitizationService>((_, args) => args![0]);
        Func<Task> consume;
        if (batched)
        {
            var message = new ProcessAuctionImportBatchCommand
            {
                CorrelationId = correlationId, SellerId = sellerId, SellerUsername = "seller", Currency = "EUR",
                Rows = rows, BatchNumber = 1, TotalBatches = 1, TotalRows = rows.Count
            };
            var consumer = new ImportAuctionsBatchConsumer(workflow, bulk, sanitizer, unit,
                NullLogger<ImportAuctionsBatchConsumer>.Instance);
            var context = Context(message, published);
            consume = () => consumer.Consume(context);
        }
        else
        {
            var message = new ProcessAuctionImportCommand
            {
                CorrelationId = correlationId, SellerId = sellerId, SellerUsername = "seller", Currency = "EUR", Rows = rows
            };
            var consumer = new ImportAuctionsConsumer(workflow, bulk, sanitizer, unit,
                NullLogger<ImportAuctionsConsumer>.Instance);
            var context = Context(message, published);
            consume = () => consumer.Consume(context);
        }

        await consume();
        var publicationCount = published.Count;
        await consume();

        Assert.Equal(new[] { "insert", "save" }, writes);
        Assert.Equal(publicationCount, published.Count);
        var progress = Assert.Single(published.OfType<ReportJobBatchProgressCommand>());
        Assert.Equal(1, progress.CompletedCount);
        Assert.Equal(1, progress.FailedCount);
        var completion = Assert.Single(published.OfType<AuctionImportCompletedEvent>());
        Assert.Equal(correlationId, completion.CorrelationId);
        Assert.Equal(1, completion.SucceededCount);
        Assert.Equal(1, completion.FailedCount);
        Assert.NotEmpty(completion.Errors);
        Assert.All(completion.Errors, error => Assert.Equal(88, error.RowNumber));
    }

    private static ConsumeContext<T> Context<T>(T message, List<object> published) where T : class =>
        TestProxy.Create<ConsumeContext<T>>((method, args) =>
        {
            switch (method.Name)
            {
                case "get_Message": return message;
                case "get_CancellationToken": return CancellationToken.None;
                case "Publish":
                    published.Add(args![0]!);
                    return Task.CompletedTask;
                default: throw new NotSupportedException(method.Name);
            }
        });
}

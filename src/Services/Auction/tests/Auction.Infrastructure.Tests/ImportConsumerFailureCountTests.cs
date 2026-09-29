using System.Reflection;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ImportAuctions;
using Auctions.Application.Interfaces;
using Auctions.Infrastructure.Messaging.Consumers;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Auction.Infrastructure.Tests;

public class ImportConsumerFailureCountTests
{
    [Theory]
    [InlineData("en-US")]
    [InlineData("ja-JP")]
    public async Task SingleImportWithMultipleErrors_CountsOneFailedRow(string culture)
    {
        var message = new ProcessAuctionImportCommand
        {
            Culture = culture,
            CorrelationId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            Rows =
            [
                new ImportAuctionItemPayload
                {
                    RowNumber = 1,
                    Title = "",
                    Description = "",
                    ReservePrice = -1m,
                    AuctionEnd = DateTimeOffset.UtcNow.AddDays(-1)
                }
            ]
        };
        var published = new List<object>();
        var context = DispatchProxy.Create<ConsumeContext<ProcessAuctionImportCommand>, TestProxy>();
        ((TestProxy)(object)context).Handler = (method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => RecordPublished(args!),
            _ => throw new NotSupportedException(method.Name)
        };
        Task RecordPublished(object?[] args)
        {
            published.Add(args[0]!);
            return Task.CompletedTask;
        }
        var consumer = new ImportAuctionsConsumer(new TestWorkflowStore(), null!, null!, null!,
            NullLogger<ImportAuctionsConsumer>.Instance);

        await consumer.Consume(context);

        var completed = Assert.Single(published.OfType<AuctionImportCompletedEvent>());
        Assert.Equal(1, completed.FailedCount);
        Assert.True(completed.Errors.Count > 1);
        Assert.Equal(culture == "ja-JP" ? "タイトルは必須です。" : "Title is required.",
            completed.Errors.First(error => error.Field == "Title").ErrorMessage);
        var failure = Assert.Single(published.OfType<FailJobByCorrelationCommand>());
        Assert.Equal("All 1 rows failed validation.", failure.ErrorMessage);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("ja-JP")]
    public async Task InvalidRowWithMultipleErrors_CountsOneFailedRow(string culture)
    {
        var message = new ProcessAuctionImportBatchCommand
        {
            Culture = culture,
            CorrelationId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            BatchNumber = 1,
            TotalBatches = 1,
            TotalRows = 1,
            Rows =
            [
                new ImportAuctionItemPayload
                {
                    RowNumber = 1,
                    Title = "",
                    Description = "",
                    ReservePrice = -1m,
                    AuctionEnd = DateTimeOffset.UtcNow.AddDays(-1)
                }
            ]
        };
        var published = new List<object>();
        var context = DispatchProxy.Create<ConsumeContext<ProcessAuctionImportBatchCommand>, TestProxy>();
        ((TestProxy)(object)context).Handler = (method, args) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => RecordPublished(args!),
            _ => throw new NotSupportedException(method.Name)
        };
        Task RecordPublished(object?[] args)
        {
            published.Add(args[0]!);
            return Task.CompletedTask;
        }

        var consumer = new ImportAuctionsBatchConsumer(new TestWorkflowStore(), null!, null!, null!,
            NullLogger<ImportAuctionsBatchConsumer>.Instance);

        await consumer.Consume(context);

        var progress = Assert.Single(published.OfType<ReportJobBatchProgressCommand>());
        Assert.Equal(1, progress.FailedCount);
        var completed = Assert.Single(published.OfType<AuctionImportCompletedEvent>());
        Assert.Equal(1, completed.FailedCount);
        Assert.True(completed.Errors.Count > 1);
        Assert.Equal(culture == "ja-JP" ? "タイトルは必須です。" : "Title is required.",
            completed.Errors.First(error => error.Field == "Title").ErrorMessage);
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}

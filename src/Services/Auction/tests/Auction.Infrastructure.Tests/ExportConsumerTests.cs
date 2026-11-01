using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuctionService.Contracts.Commands;
using AuctionService.Contracts.Events;
using Auctions.Application.Features.Auctions.ExportAuctions;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using Auctions.Domain.Enums;
using Auctions.Infrastructure.Messaging.Consumers;
using Auctions.Infrastructure.Services;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using StorageService.Contracts.Reports;
using TestSupport;
using Xunit;
using AuctionEntity = Auctions.Domain.Entities.Auction;

namespace Auction.Infrastructure.Tests;

public class ExportConsumerTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task ExportPreservesFiltersStorageMetadataAndParentJobRouting(bool hasParent, bool uploadFailsFirst)
    {
        var message = Message(hasParent);
        using var cancellation = new CancellationTokenSource();
        var item = Item.Create(new CreateItemParams
        {
            Title = "Item", Description = "Description", Condition = "Used", CategoryName = "Category"
        });
        var auction = AuctionEntity.Create(new CreateAuctionParams(
            Guid.NewGuid(), "seller", item, 10, DateTimeOffset.UtcNow.AddDays(1), "EUR"));
        auction.UpdateHighBid(15);
        var reads = 0;
        var repository = TestProxy.Create<IAuctionReadRepository>((method, args) =>
        {
            Assert.Equal(nameof(IAuctionReadRepository.GetAuctionsForExportAsync), method.Name);
            Assert.Equal(Status.Live, args![0]);
            Assert.Equal(message.SellerFilter, args[1]);
            Assert.Equal(message.StartDate, args[2]);
            Assert.Equal(message.EndDate, args[3]);
            Assert.Equal(cancellation.Token, args[4]);
            reads++;
            return Task.FromResult(new List<AuctionEntity> { auction });
        });
        var published = new List<object>();
        var context = Context(message, published, cancellation.Token);
        using var transport = new ReportHandler(message.CorrelationId, uploadFailsFirst);
        using var http = new HttpClient(transport);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ReportStorage:BaseUrl"] = "https://storage.test", ["ReportStorage:ApiKey"] = "test-key"
        }).Build();
        var consumer = new ExportAuctionsConsumer(new TestWorkflowStore(), repository, [new JsonReportExporter()],
            new AuctionExportStorageClient(http, configuration), NullLogger<ExportAuctionsConsumer>.Instance);

        if (uploadFailsFirst)
        {
            await Assert.ThrowsAsync<HttpRequestException>(() => consumer.Consume(context));
            Assert.Empty(published.OfType<AuctionExportCompletedEvent>());
            Assert.Empty(published.OfType<ReportJobItemResultCommand>());
            Assert.Empty(published.OfType<ReportJobBatchProgressCommand>());
        }

        await consumer.Consume(context);
        var publicationCount = published.Count;
        await consumer.Consume(context);

        var expectedAttempts = uploadFailsFirst ? 2 : 1;
        Assert.Equal(expectedAttempts, reads);
        Assert.Equal(expectedAttempts, transport.Uploads);
        Assert.Equal(publicationCount, published.Count);
        using var report = JsonDocument.Parse(transport.Content!);
        var row = Assert.Single(report.RootElement.GetProperty("records").EnumerateArray());
        Assert.Equal(auction.Id, row.GetProperty("auctionId").GetGuid());
        Assert.Equal("Item", row.GetProperty("title").GetString());
        Assert.Equal("seller", row.GetProperty("seller").GetString());
        Assert.Equal("Live", row.GetProperty("status").GetString());
        Assert.Equal("EUR", row.GetProperty("currency").GetString());
        Assert.Equal(10, row.GetProperty("reservePrice").GetDecimal());
        Assert.Equal(15, row.GetProperty("currentHighBid").GetDecimal());
        Assert.Equal(auction.CreatedAt, row.GetProperty("createdAt").GetDateTimeOffset());
        Assert.Equal(auction.AuctionEnd, row.GetProperty("auctionEnd").GetDateTimeOffset());
        Assert.Equal("Category", row.GetProperty("category").GetString());
        Assert.Equal("Used", row.GetProperty("condition").GetString());
        var completed = Assert.Single(published.OfType<AuctionExportCompletedEvent>());
        Assert.Equal(message.CorrelationId, completed.CorrelationId);
        Assert.Equal("stored.json", completed.FileName);
        Assert.Equal("application/json", completed.ContentType);
        Assert.Equal(321, completed.FileSizeBytes);
        Assert.Equal(7, completed.TotalRecords);
        Assert.Equal("/reports/stored/download", completed.DownloadUrl);
        Assert.Equal(hasParent ? 0 : expectedAttempts, published.OfType<RequestJobCommand>().Count());
        Assert.Equal(hasParent ? 0 : 1, published.OfType<ReportJobBatchProgressCommand>().Count());
        if (hasParent)
        {
            var result = Assert.Single(published.OfType<ReportJobItemResultCommand>());
            Assert.Equal(message.ParentJobId, result.JobId);
            Assert.Equal(message.ParentJobItemId, result.JobItemId);
            Assert.Equal(message.Attempt, result.Attempt);
            Assert.True(result.IsSuccess);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingExporterReportsFailureAndCompletesReceiptWithoutReadingOrUploading(bool hasParent)
    {
        var message = Message(hasParent);
        var published = new List<object>();
        var context = Context(message, published, default);
        var consumer = new ExportAuctionsConsumer(new TestWorkflowStore(), null!, [], null!,
            NullLogger<ExportAuctionsConsumer>.Instance);

        await consumer.Consume(context);
        var publicationCount = published.Count;
        await consumer.Consume(context);

        Assert.Equal(publicationCount, published.Count);
        var completed = Assert.Single(published.OfType<AuctionExportCompletedEvent>());
        Assert.Equal(0, completed.TotalRecords);
        Assert.Equal(string.Empty, completed.DownloadUrl);
        Assert.Empty(published.OfType<ReportJobBatchProgressCommand>());
        if (hasParent)
        {
            Assert.Empty(published.OfType<RequestJobCommand>());
            Assert.Empty(published.OfType<FailJobByCorrelationCommand>());
            var result = Assert.Single(published.OfType<ReportJobItemResultCommand>());
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFinalFailure);
            Assert.Equal(message.ParentJobId, result.JobId);
            Assert.Equal(message.ParentJobItemId, result.JobItemId);
            Assert.Equal(message.Attempt, result.Attempt);
        }
        else
        {
            Assert.Single(published.OfType<RequestJobCommand>());
            Assert.Single(published.OfType<FailJobByCorrelationCommand>());
            Assert.Empty(published.OfType<ReportJobItemResultCommand>());
        }
    }

    private static ProcessAuctionExportCommand Message(bool hasParent) => new()
    {
        CorrelationId = Guid.NewGuid(), RequestedBy = Guid.NewGuid(), Format = "Json",
        StatusFilter = "live", SellerFilter = "seller", StartDate = DateTimeOffset.UtcNow.AddDays(-1),
        EndDate = DateTimeOffset.UtcNow, ParentJobId = hasParent ? Guid.NewGuid() : null,
        ParentJobItemId = hasParent ? Guid.NewGuid() : null, Attempt = 3
    };

    private static ConsumeContext<ProcessAuctionExportCommand> Context(
        ProcessAuctionExportCommand message, List<object> published, CancellationToken token) =>
        TestProxy.Create<ConsumeContext<ProcessAuctionExportCommand>>((method, args) =>
        {
            switch (method.Name)
            {
                case "get_Message": return message;
                case "get_CancellationToken": return token;
                case "Publish":
                    published.Add(args![0]!);
                    return Task.CompletedTask;
                default: throw new NotSupportedException(method.Name);
            }
        });

    private sealed class ReportHandler(Guid correlationId, bool failFirst) : HttpMessageHandler
    {
        public byte[]? Content { get; private set; }
        public int Uploads { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal(correlationId.ToString(), request.Headers.GetValues(ReportStorageContract.RequestIdHeader).Single());
            Content = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
            Uploads++;
            if (failFirst && Uploads == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new StoredReportResponse(Guid.NewGuid(), "/reports/stored/download",
                    "stored.json", "application/json", 321, 7))
            };
        }
    }
}

using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Messaging.Consumers;
using PaymentService.Contracts.Commands;
using PaymentService.Contracts.Events;
using TestSupport;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class JobReportExecutionTests
{
    [Fact]
    public async Task ParentJob_CompletesOnlyAfterStorageAndDoesNotCreateAnotherJob()
    {
        var fixture = new Fixture();
        await fixture.Consume(fixture.Command);

        Assert.True(fixture.Stored);
        var result = Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.Command.ParentJobId, result.JobId);
        Assert.Equal(fixture.Command.ParentJobItemId, result.JobItemId);
        Assert.Equal(2, result.Attempt);
        Assert.Empty(fixture.Published.OfType<RequestJobCommand>());
        Assert.Empty(fixture.Published.OfType<ReportJobBatchProgressCommand>());
        var completion = Assert.Single(fixture.Published.OfType<OrderReportGeneratedEvent>());
        Assert.Equal("/files/report/download", completion.DownloadUrl);
        Assert.Equal(fixture.Command.RequestedBy, completion.RequestedBy);
        Assert.Equal("vi-VN", fixture.Parameters!.Culture);
    }

    [Theory]
    [InlineData("999", "Csv", null)]
    [InlineData("OrderSummary", "999", null)]
    [InlineData("OrderSummary", "Csv", "invalid")]
    [InlineData("OrderSummary", "Csv", "999")]
    public async Task InvalidReportPayload_FailsParentItemWithoutGeneratingReport(string type, string format, string? status)
    {
        var fixture = new Fixture();
        await fixture.Consume(fixture.Command with { ReportType = type, Format = format, StatusFilter = status });
        Assert.Null(fixture.Parameters);
        Assert.False(fixture.Stored);
        var result = Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFinalFailure);
        Assert.Equal(2, result.Attempt);
        Assert.Empty(fixture.Published.OfType<FailJobByCorrelationCommand>());
    }

    [Fact]
    public async Task InvalidDateRange_FailsParentItem()
    {
        var fixture = new Fixture();
        var date = DateTimeOffset.UtcNow;
        await fixture.Consume(fixture.Command with { StartDate = date, EndDate = date.AddDays(-1) });
        Assert.Null(fixture.Parameters);
        Assert.False(Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>()).IsSuccess);
    }

    [Fact]
    public async Task GeneratorFailure_IsReportedToParentItem()
    {
        var fixture = new Fixture { GeneratorSucceeds = false };
        await fixture.Consume(fixture.Command);
        var result = Assert.Single(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFinalFailure);
        Assert.False(fixture.Stored);
        Assert.Empty(fixture.Published.OfType<OrderReportGeneratedEvent>());
    }

    [Fact]
    public async Task StorageFailure_PropagatesForBrokerRetryWithoutCompletingItem()
    {
        var fixture = new Fixture { StorageFails = true };
        await Assert.ThrowsAsync<HttpRequestException>(() => fixture.Consume(fixture.Command));
        Assert.Empty(fixture.Published.OfType<ReportJobItemResultCommand>());
        Assert.Empty(fixture.Published.OfType<OrderReportGeneratedEvent>());
    }

    [Fact]
    public async Task StandaloneReport_PreservesExistingProgressProtocol()
    {
        var fixture = new Fixture();
        await fixture.Consume(fixture.Command with { ParentJobId = null, ParentJobItemId = null });
        Assert.Single(fixture.Published.OfType<RequestJobCommand>());
        Assert.Equal(1, Assert.Single(fixture.Published.OfType<ReportJobBatchProgressCommand>()).CompletedCount);
        Assert.Empty(fixture.Published.OfType<ReportJobItemResultCommand>());
    }

    private sealed class Fixture
    {
        public GenerateOrderReportCommand Command { get; } = new()
        {
            ParentJobId = Guid.NewGuid(), ParentJobItemId = Guid.NewGuid(), Attempt = 2,
            CorrelationId = Guid.NewGuid(), RequestedBy = Guid.NewGuid(),
            ReportType = "OrderSummary", Format = "Csv", Culture = "vi-VN"
        };
        public List<object> Published { get; } = [];
        public OrderReportParameters? Parameters { get; private set; }
        public bool Stored { get; private set; }
        public bool GeneratorSucceeds { get; init; } = true;
        public bool StorageFails { get; init; }

        public Task Consume(GenerateOrderReportCommand command)
        {
            var context = TestProxy.Create<ConsumeContext<GenerateOrderReportCommand>>((method, args) => method.Name switch
            {
                "get_Message" => command,
                "get_CancellationToken" => CancellationToken.None,
                "Publish" => Capture(args![0]!),
                _ => throw new NotSupportedException(method.Name)
            });
            var generator = TestProxy.Create<IOrderReportGenerator>((method, args) =>
            {
                Parameters = (OrderReportParameters)args![2]!;
                return Task.FromResult(new OrderReportResult(GeneratorSucceeds, "report.csv", "text/csv", [1], 1, 1,
                    GeneratorSucceeds ? null : "Generation failed"));
            });
            var storage = TestProxy.Create<IReportStorageClient>((method, args) =>
            {
                Assert.Equal(command.RequestedBy, args![1]);
                Assert.Empty(Published.OfType<ReportJobItemResultCommand>());
                if (StorageFails) return Task.FromException<string>(new HttpRequestException("storage unavailable"));
                Stored = true;
                return Task.FromResult("/files/report/download");
            });
            return new GenerateOrderReportConsumer(generator, storage, null!,
                NullLogger<GenerateOrderReportConsumer>.Instance).Consume(context);
        }

        private Task Capture(object message) { Published.Add(message); return Task.CompletedTask; }
    }
}

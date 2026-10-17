using JobService.Contracts.Commands;
using Jobs.Infrastructure.Messaging.Consumers;
using MassTransit;
using NotificationService.Contracts.Commands;
using PaymentService.Contracts.Commands;
using StorageService.Contracts.Commands;
using TestSupport;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public class ServiceJobFaultTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StorageFault_ReportsParentItemAndAttempt(bool image)
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var published = new List<object>();
        var consumer = new ServiceJobFaultConsumer();
        if (image)
            await consumer.Consume(Context(new ProcessImageCommand
            {
                ParentJobId = jobId, ParentJobItemId = itemId, Attempt = 2
            }, published));
        else
            await consumer.Consume(Context(new FinalizeFileUploadCommand
            {
                ParentJobId = jobId, ParentJobItemId = itemId, Attempt = 2
            }, published));
        var result = Assert.IsType<ReportJobItemResultCommand>(Assert.Single(published));
        Assert.Equal(jobId, result.JobId);
        Assert.Equal(itemId, result.JobItemId);
        Assert.Equal(2, result.Attempt);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFinalFailure);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task BrokerFault_ReportsParentItemAndAttempt(bool report)
    {
        var jobId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var published = new List<object>();
        var consumer = new ServiceJobFaultConsumer();
        if (report)
            await consumer.Consume(Context(new GenerateOrderReportCommand
            {
                ParentJobId = jobId, ParentJobItemId = itemId, Attempt = 3
            }, published));
        else
            await consumer.Consume(Context(new SendBulkNotificationCommand
            {
                ParentJobId = jobId, ParentJobItemId = itemId, Attempt = 3
            }, published));

        var result = Assert.IsType<ReportJobItemResultCommand>(Assert.Single(published));
        Assert.Equal(jobId, result.JobId);
        Assert.Equal(itemId, result.JobItemId);
        Assert.Equal(3, result.Attempt);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFinalFailure);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StandaloneFault_UsesCorrelationProtocol(bool report)
    {
        var correlation = Guid.NewGuid();
        var published = new List<object>();
        var consumer = new ServiceJobFaultConsumer();
        if (report)
            await consumer.Consume(Context(new GenerateOrderReportCommand { CorrelationId = correlation }, published));
        else
            await consumer.Consume(Context(new SendBulkNotificationCommand { CorrelationId = correlation }, published));
        var result = Assert.IsType<FailJobByCorrelationCommand>(Assert.Single(published));
        Assert.Equal(correlation.ToString(), result.CorrelationId);
    }

    private static ConsumeContext<Fault<T>> Context<T>(T command, List<object> published) where T : class
    {
        var fault = TestProxy.Create<Fault<T>>((method, _) => method.Name switch
        {
            "get_Message" => command,
            _ => throw new NotSupportedException(method.Name)
        });
        return TestProxy.Create<ConsumeContext<Fault<T>>>((method, args) => method.Name switch
        {
            "get_Message" => fault,
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => Capture(args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task Capture(object message) { published.Add(message); return Task.CompletedTask; }
    }
}

using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Application.Interfaces;
using Payment.Infrastructure.Messaging.Consumers;
using Payment.Infrastructure.Services;
using PaymentService.Contracts.Commands;
using PaymentService.Contracts.Events;
using StorageService.Contracts.Reports;
using Xunit;

namespace Payment.Infrastructure.Tests;

public class ReportStorageClientTests
{
    [Fact]
    public async Task StoreAsync_SendsReportAndReturnsDownloadUrl()
    {
        var ownerId = Guid.NewGuid();
        var report = new OrderReportResult(true, "report.csv", "text/csv", [1, 2], 2, 1);
        var handler = new RecordingHttpHandler();
        using var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ReportStorage:BaseUrl"] = "http://storage-api:8080",
                ["ReportStorage:ApiKey"] = "test-internal-key"
            }).Build();
        var client = new HttpReportStorageClient(httpClient, configuration);

        var url = await client.StoreAsync(report, ownerId, CancellationToken.None);

        Assert.Equal("/files/report-id/download", url);
        Assert.Equal(ReportStorageContract.UploadPath, handler.Path);
        Assert.Equal(ownerId.ToString(), handler.OwnerId);
        Assert.Equal("report.csv", handler.FileName);
        Assert.Equal("test-internal-key", handler.ApiKey);
        Assert.Equal("text/csv", handler.ContentType);
        Assert.Equal(report.Content, handler.Content);
    }

    [Fact]
    public async Task Consumer_DoesNotPublishCompletionWhenStorageFails()
    {
        var published = new List<object>();
        var context = DispatchProxy.Create<ConsumeContext<GenerateOrderReportCommand>, TestProxy>();
        ((TestProxy)(object)context).Handler = (method, args) => method.Name switch
        {
            "get_Message" => new GenerateOrderReportCommand
            {
                CorrelationId = Guid.NewGuid(),
                RequestedBy = Guid.NewGuid(),
                ReportType = nameof(ReportType.OrderSummary),
                Format = nameof(ReportFormat.Csv)
            },
            "get_CancellationToken" => CancellationToken.None,
            "Publish" => RecordPublished(args!),
            _ => throw new NotSupportedException(method.Name)
        };
        Task RecordPublished(object?[] args)
        {
            published.Add(args[0]!);
            return Task.CompletedTask;
        }

        var generator = DispatchProxy.Create<IOrderReportGenerator, TestProxy>();
        ((TestProxy)(object)generator).Handler = (_, _) => Task.FromResult(
            new OrderReportResult(true, "report.csv", "text/csv", [1], 1, 1));
        var storage = DispatchProxy.Create<IReportStorageClient, TestProxy>();
        ((TestProxy)(object)storage).Handler = (_, _) =>
            Task.FromException<string>(new HttpRequestException("storage unavailable"));
        var consumer = new GenerateOrderReportConsumer(generator, storage, null!,
            NullLogger<GenerateOrderReportConsumer>.Instance);

        await Assert.ThrowsAsync<HttpRequestException>(() => consumer.Consume(context));

        Assert.DoesNotContain(published, message => message is OrderReportGeneratedEvent);
    }

    private sealed class RecordingHttpHandler : HttpMessageHandler
    {
        public string? Path { get; private set; }
        public string? OwnerId { get; private set; }
        public string? FileName { get; private set; }
        public string? ApiKey { get; private set; }
        public string? ContentType { get; private set; }
        public byte[]? Content { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Path = request.RequestUri?.AbsolutePath;
            OwnerId = request.Headers.GetValues(ReportStorageContract.OwnerIdHeader).Single();
            FileName = request.Headers.GetValues(ReportStorageContract.FileNameHeader).Single();
            ApiKey = request.Headers.GetValues(ReportStorageContract.ApiKeyHeader).Single();
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            Content = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new StoredReportResponse(Guid.NewGuid(), "/files/report-id/download")),
                    Encoding.UTF8, "application/json")
            };
        }
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}

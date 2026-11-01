using System.Text.Json;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Storage.Infrastructure.Messaging.Consumers;
using StorageService.Contracts.Commands;
using StorageService.Contracts.Events;
using TestSupport;
using Xunit;

namespace Storage.Infrastructure.Tests;

public class ImageProcessingTriggerConsumerTests
{
    [Fact]
    public async Task UploadTrigger_QueuesFileIdWithoutTrustingEventUrlAndSkipsUnownedFiles()
    {
        var messages = new List<RequestJobCommand>();
        var publisher = TestProxy.Create<IPublishEndpoint>((method, args) =>
        {
            messages.Add(Assert.IsType<RequestJobCommand>(args![0]));
            return Task.CompletedTask;
        });
        var consumer = new ImageProcessingTriggerConsumer(publisher, NullLogger<ImageProcessingTriggerConsumer>.Instance);
        var upload = new FileUploadedEvent
        {
            FileId = Guid.NewGuid(), OwnerId = Guid.NewGuid(), ContentType = "image/png", Url = "https://untrusted.invalid/file"
        };
        await consumer.Consume(Context(upload));
        var request = Assert.Single(messages);
        Assert.Equal("ImageProcessing", request.JobType);
        Assert.Equal(upload.OwnerId, request.RequestedBy);
        var item = Assert.Single(request.Items);
        Assert.Equal(upload.FileId, JsonSerializer.Deserialize<ProcessImageCommand>(item.PayloadJson)!.FileId);
        Assert.DoesNotContain("untrusted.invalid", item.PayloadJson);
        await consumer.Consume(Context(upload with { OwnerId = null }));
        await consumer.Consume(Context(upload with { ContentType = "application/pdf" }));
        Assert.Single(messages);
    }

    private static ConsumeContext<FileUploadedEvent> Context(FileUploadedEvent message) =>
        TestProxy.Create<ConsumeContext<FileUploadedEvent>>((method, _) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            _ => throw new NotSupportedException(method.Name)
        });
}

using System.Text.Json;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Storage.Application.Features.Files.GetFileById;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Storage.Infrastructure.Messaging.Consumers;
using StorageService.Contracts.Commands;
using StorageService.Contracts.Events;
using TestSupport;
using Xunit;

namespace Storage.Application.Tests;

public class StorageImageMetadataTests
{
    [Fact]
    public async Task GetFile_ReturnsProcessedImageAndChecksum()
    {
        var file = StoredFile.Create("image.png", "image.png", "image/png", 12, "/image.png", null, Guid.NewGuid(), StorageProvider.Local);
        file.RecordProcessedImage("checksum", 640, 320, "data:image/jpeg;base64,dGVzdA==");
        var repository = TestProxy.Create<IStoredFileRepository>((_, _) => Task.FromResult<StoredFile?>(file));
        var handler = new GetFileByIdQueryHandler(repository, NullLogger<GetFileByIdQueryHandler>.Instance);
        var result = await handler.Handle(new GetFileByIdQuery(file.Id), default);
        Assert.True(result.IsSuccess);
        Assert.Equal("checksum", result.Value!.Checksum);
        Assert.Equal(640, result.Value.Image!.Width);
        Assert.Equal(320, result.Value.Image.Height);
        Assert.Equal("data:image/jpeg;base64,dGVzdA==", result.Value.Image.ThumbnailDataUrl);
    }

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

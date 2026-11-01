using Microsoft.Extensions.Logging.Abstractions;
using Storage.Application.Features.Files.GetFileById;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using TestSupport;
using Xunit;

namespace Storage.Application.Tests;

public class StorageImageMetadataTests
{
    [Fact]
    public async Task GetFile_ReturnsProcessedImageAndChecksum()
    {
        var file = StoredFile.Create(new CreateStoredFileParams
        {
            FileName = "image.png", StoredFileName = "image.png", ContentType = "image/png", FileSize = 12,
            Url = "/image.png", OwnerId = Guid.NewGuid(), Provider = StorageProvider.Local
        });
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

}

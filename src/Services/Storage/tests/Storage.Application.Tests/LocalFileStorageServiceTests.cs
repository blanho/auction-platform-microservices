using System.Text;
using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Xunit;

namespace Storage.Application.Tests;

public sealed class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _storageRoot = Path.Combine(
        Path.GetTempPath(),
        $"auction-storage-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task UploadAsync_NormalizesPathAndEscapesUrlSegments()
    {
        var service = CreateService(baseUrl: "/files/");
        await using var content = CreateContent("image-content");

        var result = await service.UploadAsync(new FileUploadRequest(
            content,
            "photo.PNG",
            "image/png",
            content.Length,
            @"avatars\sellers//Jane Doe"));

        Assert.StartsWith("avatars/sellers/Jane Doe/", result.StoredFileName, StringComparison.Ordinal);
        Assert.EndsWith(".png", result.StoredFileName, StringComparison.Ordinal);
        Assert.Equal($"/files/{result.StoredFileName.Replace("Jane Doe", "Jane%20Doe")}", result.Url);

        var diskPath = Path.Combine(
            _storageRoot,
            result.StoredFileName.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(diskPath));
        Assert.Equal("image-content", await File.ReadAllTextAsync(diskPath));
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("safe/../../outside")]
    [InlineData(@"..\outside")]
    [InlineData("%2e%2e/outside")]
    [InlineData("/absolute")]
    [InlineData(@"C:\outside")]
    public async Task UploadAsync_RejectsPathsOutsideStorageRoot(string subFolder)
    {
        var service = CreateService();
        await using var content = CreateContent("blocked");
        var request = new FileUploadRequest(
            content,
            "file.txt",
            "text/plain",
            content.Length,
            subFolder);

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadAsync(request));

        Assert.Empty(Directory.EnumerateFiles(_storageRoot, "*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task FileOperations_RejectSiblingPrefixTraversal()
    {
        var service = CreateService();
        var siblingDirectory = $"{_storageRoot}-outside";
        Directory.CreateDirectory(siblingDirectory);
        var siblingFile = Path.Combine(siblingDirectory, "secret.txt");
        await File.WriteAllTextAsync(siblingFile, "secret");
        var traversalPath = $"../{Path.GetFileName(siblingDirectory)}/secret.txt";

        try
        {
            Assert.False(await service.ExistsAsync(traversalPath));
            Assert.Null(await service.DownloadAsync(traversalPath));
            Assert.Null(await service.GetUrlAsync(traversalPath));
            Assert.Null(await service.GenerateDownloadSasTokenAsync(traversalPath));
            Assert.False(await service.DeleteAsync(traversalPath));
            Assert.True(File.Exists(siblingFile));
        }
        finally
        {
            Directory.Delete(siblingDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task FileOperations_UseCanonicalStoredPath()
    {
        var service = CreateService(baseUrl: "https://storage.example.test/files/");
        await using var content = CreateContent("stored-content");
        var upload = await service.UploadAsync(new FileUploadRequest(
            content,
            "document.PDF",
            "application/pdf",
            content.Length,
            "user documents"));

        Assert.True(await service.ExistsAsync(upload.StoredFileName));
        Assert.Equal(
            $"https://storage.example.test/files/{upload.StoredFileName.Replace("user documents", "user%20documents")}",
            await service.GetUrlAsync(upload.StoredFileName));

        var presignedDownload = await service.GenerateDownloadSasTokenAsync(upload.StoredFileName);
        Assert.NotNull(presignedDownload);
        Assert.Equal("application/pdf", presignedDownload.ContentType);
        Assert.Equal(Path.GetFileName(upload.StoredFileName), presignedDownload.FileName);

        var download = await service.DownloadAsync(upload.StoredFileName);
        Assert.NotNull(download);
        await using (download.Content)
        using (var reader = new StreamReader(download.Content, Encoding.UTF8))
        {
            Assert.Equal("stored-content", await reader.ReadToEndAsync());
        }

        Assert.True(await service.DeleteAsync(upload.StoredFileName));
        Assert.False(await service.ExistsAsync(upload.StoredFileName));
    }

    [Fact]
    public async Task FileOperations_RejectSymbolicLinkEscapes()
    {
        var service = CreateService();
        var outsideDirectory = Path.Combine(
            Path.GetTempPath(),
            $"auction-storage-outside-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outsideDirectory);
        var outsideFile = Path.Combine(outsideDirectory, "secret.txt");
        await File.WriteAllTextAsync(outsideFile, "secret");
        var linkPath = Path.Combine(_storageRoot, "linked");

        try
        {
            try
            {
                Directory.CreateSymbolicLink(linkPath, outsideDirectory);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
            {
                return;
            }

            Assert.False(await service.ExistsAsync("linked/secret.txt"));
            Assert.Null(await service.DownloadAsync("linked/secret.txt"));
            Assert.False(await service.DeleteAsync("linked/secret.txt"));
            Assert.True(File.Exists(outsideFile));

            await using var content = CreateContent("blocked");
            var request = new FileUploadRequest(
                content,
                "file.txt",
                "text/plain",
                content.Length,
                "linked");
            await Assert.ThrowsAsync<ArgumentException>(() => service.UploadAsync(request));
        }
        finally
        {
            if (Directory.Exists(linkPath))
            {
                Directory.Delete(linkPath);
            }

            Directory.Delete(outsideDirectory, recursive: true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_storageRoot))
        {
            Directory.Delete(_storageRoot, recursive: true);
        }
    }

    private LocalFileStorageService CreateService(string baseUrl = "/files")
    {
        var settings = Options.Create(new FileStorageSettings
        {
            Provider = "Local",
            Local = new LocalStorageSettings
            {
                BasePath = _storageRoot,
                BaseUrl = baseUrl
            }
        });

        return new LocalFileStorageService(
            settings,
            new RecyclableMemoryStreamManager(),
            NullLogger<LocalFileStorageService>.Instance);
    }

    private static MemoryStream CreateContent(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
}

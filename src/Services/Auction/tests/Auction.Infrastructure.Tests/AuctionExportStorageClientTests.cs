using System.Net;
using System.Text;
using System.Text.Json;
using Auctions.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using StorageService.Contracts.Reports;
using Xunit;

namespace Auction.Infrastructure.Tests;

public class AuctionExportStorageClientTests
{
    [Theory]
    [InlineData("auctions.csv", "text/csv")]
    [InlineData("auctions.json", "application/json")]
    public async Task StoreAsync_UploadsExportAndReturnsDownloadUrl(string fileName, string contentType)
    {
        var ownerId = Guid.NewGuid();
        byte[] content = [1, 2, 3];
        var handler = new RecordingHttpHandler();
        using var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ReportStorage:BaseUrl"] = "http://storage-api:8080",
                ["ReportStorage:ApiKey"] = "test-internal-key"
            }).Build();
        var client = new AuctionExportStorageClient(httpClient, configuration);

        var url = await client.StoreAsync(content, fileName, contentType, ownerId, CancellationToken.None);

        Assert.Equal("/files/report-id/download", url);
        Assert.Equal(ReportStorageContract.UploadPath, handler.Path);
        Assert.Equal(ownerId.ToString(), handler.OwnerId);
        Assert.Equal(fileName, handler.FileName);
        Assert.Equal("test-internal-key", handler.ApiKey);
        Assert.Equal(contentType, handler.ContentType);
        Assert.Equal(content, handler.Content);
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
}

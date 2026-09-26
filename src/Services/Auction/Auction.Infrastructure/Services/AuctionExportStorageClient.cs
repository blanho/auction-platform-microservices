using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using StorageService.Contracts.Reports;

namespace Auctions.Infrastructure.Services;

public sealed class AuctionExportStorageClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AuctionExportStorageClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration["ReportStorage:BaseUrl"]
            ?? throw new InvalidOperationException("ReportStorage:BaseUrl is required."));
        _apiKey = configuration["ReportStorage:ApiKey"]
            ?? throw new InvalidOperationException("ReportStorage:ApiKey is required.");
    }

    public async Task<string> StoreAsync(
        byte[] content,
        string fileName,
        string contentType,
        Guid ownerId,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ReportStorageContract.UploadPath);
        request.Headers.Add(ReportStorageContract.ApiKeyHeader, _apiKey);
        request.Headers.Add(ReportStorageContract.FileNameHeader, fileName);
        request.Headers.Add(ReportStorageContract.OwnerIdHeader, ownerId.ToString());
        request.Content = new ByteArrayContent(content);
        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        using var response = await _httpClient.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var storedReport = await response.Content.ReadFromJsonAsync<StoredReportResponse>(cancellationToken);
        return storedReport?.DownloadUrl
            ?? throw new InvalidOperationException("Storage returned no report download URL.");
    }
}

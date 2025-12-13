using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Common.Storage.Models;

namespace Common.Storage.Clients;

public record TempUploadResult
{
    public bool Success { get; init; }
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long Size { get; init; }
    public string? Error { get; init; }
}

public record FileConfirmResult
{
    public bool Success { get; init; }
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string? Url { get; init; }
    public string? Error { get; init; }
}

public interface IFileStorageClient
{
    Task<TempUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<FileConfirmResult> ConfirmFileAsync(
        Guid fileId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<FileConfirmResult> UploadAndConfirmAsync(
        Stream stream,
        string fileName,
        string contentType,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<Stream?> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<FileMetadata?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default);

    Task<IEnumerable<FileMetadata>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}

public class FileStorageClient : IFileStorageClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileStorageClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<TempUploadResult> UploadToTempAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("api/files/upload", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new TempUploadResult { Success = false, Error = error };
        }

        var metadata = await response.Content.ReadFromJsonAsync<FileMetadata>(_jsonOptions, cancellationToken);
        
        return metadata != null
            ? new TempUploadResult 
            { 
                Success = true, 
                FileId = metadata.Id, 
                FileName = metadata.FileName,
                Size = metadata.Size
            }
            : new TempUploadResult { Success = false, Error = "Invalid response" };
    }

    public async Task<FileConfirmResult> ConfirmFileAsync(
        Guid fileId,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var request = new FileConfirmRequest
        {
            FileId = fileId,
            EntityType = entityType,
            EntityId = entityId
        };

        var response = await _httpClient.PostAsJsonAsync("api/files/confirm", request, _jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new FileConfirmResult { Success = false, FileId = fileId, Error = error };
        }

        var metadata = await response.Content.ReadFromJsonAsync<FileMetadata>(_jsonOptions, cancellationToken);
        
        return metadata != null
            ? new FileConfirmResult
            {
                Success = true,
                FileId = metadata.Id,
                FileName = metadata.FileName,
                Url = metadata.Url
            }
            : new FileConfirmResult { Success = false, FileId = fileId, Error = "Invalid response" };
    }

    public async Task<FileConfirmResult> UploadAndConfirmAsync(
        Stream stream,
        string fileName,
        string contentType,
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var uploadResult = await UploadToTempAsync(stream, fileName, contentType, cancellationToken);
        
        if (!uploadResult.Success)
        {
            return new FileConfirmResult { Success = false, Error = uploadResult.Error };
        }

        return await ConfirmFileAsync(uploadResult.FileId, entityType, entityId, cancellationToken);
    }

    public async Task<Stream?> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/files/{fileId}/download", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public async Task<FileMetadata?> GetMetadataAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<FileMetadata>($"api/files/{fileId}", _jsonOptions, cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<IEnumerable<FileMetadata>>(
            $"api/files/entity/{entityType}/{entityId}",
            _jsonOptions,
            cancellationToken);

        return result ?? Enumerable.Empty<FileMetadata>();
    }

    public async Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"api/files/{fileId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}

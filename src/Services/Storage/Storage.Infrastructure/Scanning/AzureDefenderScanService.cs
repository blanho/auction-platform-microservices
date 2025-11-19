using BuildingBlocks.Web.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Storage.Application.Configuration;
using Storage.Application.Interfaces;
using Storage.Domain.ValueObjects;
using Storage.Infrastructure.Storage;

namespace Storage.Infrastructure.Scanning;

public class AzureDefenderScanService : IVirusScanService
{
    private readonly AzureBlobStorageProvider _blobProvider;
    private readonly IStorageProvider _storageProvider;
    private readonly StorageOptions _options;
    private readonly ILogger<AzureDefenderScanService> _logger;

    private const string ScanEngineName = "Azure-Defender";

    private const string MalwareScanResultTag = "Malware Scanning scan result";
    private const string MalwareScanTimestampTag = "Malware Scanning scan time";

    public AzureDefenderScanService(
        AzureBlobStorageProvider blobProvider,
        IStorageProvider storageProvider,
        IOptions<StorageOptions> options,
        ILogger<AzureDefenderScanService> logger)
    {
        _blobProvider = blobProvider;
        _storageProvider = storageProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {

            var scanTags = await _blobProvider.GetScanTagsAsync(filePath, cancellationToken);

            if (scanTags == null)
            {
                _logger.LogWarning("Could not retrieve blob tags for {FilePath}", filePath);
                return ScanResult.Pending(ScanEngineName, "Blob not found or tags unavailable");
            }

            if (!scanTags.TryGetValue(MalwareScanResultTag, out var scanResult) &&
                !scanTags.TryGetValue(_options.Azure.ScanStatusTagKey, out scanResult))
            {

                _logger.LogDebug("No scan result tag found for {FilePath}, scan pending", filePath);
                return ScanResult.Pending(ScanEngineName, "Scan in progress");
            }

            DateTimeOffset? scannedAt = null;
            if (scanTags.TryGetValue(MalwareScanTimestampTag, out var timestampStr) &&
                DateTimeOffset.TryParse(timestampStr, out var timestamp))
            {
                scannedAt = timestamp;
            }

            var scanDuration = DateTimeOffset.UtcNow - startTime;

            if (scanResult.Equals(_options.Azure.ScanResultClean, StringComparison.OrdinalIgnoreCase) ||
                scanResult.Equals("No threats found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "File {FilePath} passed Azure Defender scan",
                    filePath);

                return ScanResult.Clean(ScanEngineName);
            }

            if (scanResult.Contains("Malicious", StringComparison.OrdinalIgnoreCase) ||
                scanResult.Contains("threat", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "SECURITY: File {FilePath} detected as malicious by Azure Defender: {Result}",
                    filePath, scanResult);

                return ScanResult.Infected(ScanEngineName, scanResult);
            }

            _logger.LogWarning(
                "Unknown scan result for {FilePath}: {Result}",
                filePath, scanResult);

            return ScanResult.Pending(ScanEngineName, $"Unknown result: {scanResult}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking scan status for {FilePath}", filePath);
            return ScanResult.Failed(ScanEngineName, ex.Message);
        }
    }

    public Task<bool> IsScanningEnabledAsync(string contentType, CancellationToken cancellationToken = default)
    {
        if (!_options.Scanning.Enabled)
        {
            return Task.FromResult(false);
        }

        if (_options.Scanning.ExemptContentTypes?.Contains(contentType, StringComparer.OrdinalIgnoreCase) == true)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task<Application.Interfaces.ScanStatus> CheckScanStatusAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var scanTags = await _blobProvider.GetScanTagsAsync(filePath, cancellationToken);

            if (scanTags == null)
            {
                return Application.Interfaces.ScanStatus.Pending;
            }

            if (!scanTags.TryGetValue(MalwareScanResultTag, out var scanResult) &&
                !scanTags.TryGetValue(_options.Azure.ScanStatusTagKey, out scanResult))
            {
                return Application.Interfaces.ScanStatus.InProgress;
            }

            if (scanResult.Equals(_options.Azure.ScanResultClean, StringComparison.OrdinalIgnoreCase) ||
                scanResult.Equals("No threats found", StringComparison.OrdinalIgnoreCase))
            {
                return Application.Interfaces.ScanStatus.Complete;
            }

            if (scanResult.Contains("Malicious", StringComparison.OrdinalIgnoreCase))
            {
                return Application.Interfaces.ScanStatus.Complete;
            }

            return Application.Interfaces.ScanStatus.InProgress;
        }
        catch
        {
            return Application.Interfaces.ScanStatus.Failed;
        }
    }

    public Task TriggerScanAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Manual scan trigger not needed for Azure Defender - scans are automatic");
        return Task.CompletedTask;
    }

    public async Task<string> QuarantineFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var quarantineFolder = _options.Azure.QuarantineContainerName ?? "auction-quarantine";
        var quarantinePath = await _storageProvider.MoveAsync(filePath, quarantineFolder, cancellationToken);

        if (quarantinePath == null)
        {
            _logger.LogError("Failed to quarantine file {FilePath}", filePath);
            throw new ConflictException($"Failed to quarantine file: {filePath}");
        }

        _logger.LogWarning("SECURITY: Quarantined file {FilePath} to {QuarantinePath}", filePath, quarantinePath);

        return quarantinePath;
    }
}

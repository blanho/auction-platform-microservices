using Storage.Domain.ValueObjects;

namespace Storage.Application.Interfaces;

public interface IVirusScanService
{

    Task<ScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default);

    Task<ScanStatus> CheckScanStatusAsync(string filePath, CancellationToken cancellationToken = default);

    Task TriggerScanAsync(string filePath, CancellationToken cancellationToken = default);

    Task<string> QuarantineFileAsync(string filePath, CancellationToken cancellationToken = default);
}

public enum ScanStatus
{
    Pending,
    InProgress,
    Complete,
    Failed
}

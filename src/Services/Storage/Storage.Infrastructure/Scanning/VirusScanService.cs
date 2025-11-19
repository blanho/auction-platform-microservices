using Microsoft.Extensions.Logging;
using Storage.Application.Interfaces;
using Storage.Domain.ValueObjects;

namespace Storage.Infrastructure.Scanning;

public class LocalVirusScanService : IVirusScanService
{
    private readonly ILogger<LocalVirusScanService> _logger;
    private readonly bool _simulateInfection;
    private readonly HashSet<string> _infectedPatterns;

    private const string ScanEngineName = "LocalDev-Stub";

    public LocalVirusScanService(
        ILogger<LocalVirusScanService> logger,
        bool simulateInfection = false)
    {
        _logger = logger;
        _simulateInfection = simulateInfection;
        _infectedPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "eicar", "virus", "malware", "infected"
        };
    }

    public Task<ScanResult> ScanFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("LocalDev: Scanning file {Path}", filePath);

        Thread.Sleep(100);

        var fileName = Path.GetFileName(filePath);
        if (_simulateInfection && _infectedPatterns.Any(p => fileName.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogWarning("LocalDev: Simulated infection detected in {Path}", filePath);
            return Task.FromResult(ScanResult.Infected(ScanEngineName, "Simulated infection for testing"));
        }

        _logger.LogDebug("LocalDev: File {Path} is clean", filePath);
        return Task.FromResult(ScanResult.Clean(ScanEngineName, null, 100));
    }

    public Task<ScanStatus> CheckScanStatusAsync(string filePath, CancellationToken cancellationToken = default)
    {

        return Task.FromResult(ScanStatus.Complete);
    }

    public Task TriggerScanAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("LocalDev: Scan trigger ignored (synchronous scanning)");
        return Task.CompletedTask;
    }

    public Task<string> QuarantineFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var quarantinePath = $"quarantine/{Path.GetFileName(filePath)}";
        _logger.LogWarning("LocalDev: Would quarantine {Source} -> {Dest}", filePath, quarantinePath);
        return Task.FromResult(quarantinePath);
    }
}

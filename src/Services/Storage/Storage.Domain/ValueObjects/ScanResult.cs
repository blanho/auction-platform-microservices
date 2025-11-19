#nullable enable

namespace Storage.Domain.ValueObjects;

public enum ScanResultStatus
{
    Clean,
    Infected,
    Error,
    Timeout,
    Pending,
    Failed
}

public class ScanResult
{
    public ScanResultStatus Status { get; set; } = ScanResultStatus.Pending;

    public string? ThreatDetails { get; set; }

    public DateTimeOffset ScannedAt { get; set; }

    public string Engine { get; set; } = string.Empty;

    public string? EngineVersion { get; set; }

    public string? DefinitionVersion { get; set; }

    public long? DurationMs { get; set; }

    public bool IsQuarantined { get; set; }

    public string? QuarantinePath { get; set; }

    public bool IsClean => Status == ScanResultStatus.Clean;

    public bool IsInfected => Status == ScanResultStatus.Infected;

    public bool IsFailed => Status == ScanResultStatus.Error || Status == ScanResultStatus.Timeout || Status == ScanResultStatus.Failed;

    public bool IsPending => Status == ScanResultStatus.Pending;

    public static ScanResult Clean(string engine, string? reason = null, long? durationMs = null, DateTimeOffset? scannedAt = null) => new()
    {
        Status = ScanResultStatus.Clean,
        ThreatDetails = reason,
        ScannedAt = scannedAt ?? DateTimeOffset.UtcNow,
        Engine = engine,
        DurationMs = durationMs
    };

    public static ScanResult Infected(string engine, string threatDetails, string? quarantinePath = null, DateTimeOffset? scannedAt = null) => new()
    {
        Status = ScanResultStatus.Infected,
        ThreatDetails = threatDetails,
        ScannedAt = scannedAt ?? DateTimeOffset.UtcNow,
        Engine = engine,
        IsQuarantined = quarantinePath != null,
        QuarantinePath = quarantinePath
    };

    public static ScanResult Error(string errorDetails, string engine, DateTimeOffset? scannedAt = null) => new()
    {
        Status = ScanResultStatus.Error,
        ThreatDetails = errorDetails,
        ScannedAt = scannedAt ?? DateTimeOffset.UtcNow,
        Engine = engine
    };

    public static ScanResult Timeout(string engine, DateTimeOffset? scannedAt = null) => new()
    {
        Status = ScanResultStatus.Timeout,
        ScannedAt = scannedAt ?? DateTimeOffset.UtcNow,
        Engine = engine
    };

    public static ScanResult Pending(string engine, string? message = null, DateTimeOffset? scannedAt = null) => new()
    {
        Status = ScanResultStatus.Pending,
        ThreatDetails = message,
        ScannedAt = scannedAt ?? DateTimeOffset.UtcNow,
        Engine = engine
    };

    public static ScanResult Failed(string engine, string errorDetails, DateTimeOffset? scannedAt = null) => new()
    {
        Status = ScanResultStatus.Failed,
        ThreatDetails = errorDetails,
        ScannedAt = scannedAt ?? DateTimeOffset.UtcNow,
        Engine = engine
    };
}

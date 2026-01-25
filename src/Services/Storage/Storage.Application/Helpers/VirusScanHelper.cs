using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Application.Helpers;

public static class VirusScanHelper
{
    public static string GetScanStatusMessage(StoredFile file)
    {
        return file.Status switch
        {
            FileStatus.Scanning => "Virus scan in progress",
            FileStatus.Scanned => "Scan complete - file is clean",
            FileStatus.Infected => $"File is infected: {file.ScanResult?.ThreatDetails ?? "Unknown threat"}",
            FileStatus.InTemp => "File uploaded, awaiting scan",
            _ => $"Status: {file.Status}"
        };
    }

    public static bool ShouldScanFile(long fileSize, string contentType, long maxScanSize, IEnumerable<string> exemptContentTypes)
    {
        if (fileSize == 0)
            return false;

        if (fileSize > maxScanSize)
            return false;

        if (exemptContentTypes.Contains(contentType))
            return false;

        return true;
    }
}

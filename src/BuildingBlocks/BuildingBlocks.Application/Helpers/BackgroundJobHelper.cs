using BuildingBlocks.Application.BackgroundJobs.Core;

namespace BuildingBlocks.Application.Helpers;

public static class BackgroundJobHelper
{
    public static int CalculateEstimatedSecondsRemaining(int processedItems, int totalItems, DateTimeOffset startTime)
    {
        if (processedItems <= 0) return 0;

        var elapsed = (DateTimeOffset.UtcNow - startTime).TotalSeconds;
        var itemsPerSecond = processedItems / elapsed;
        var remainingItems = totalItems - processedItems;
        return (int)(remainingItems / Math.Max(itemsPerSecond, 0.1));
    }

    public static async Task SaveResultFileAsync(
        BackgroundJobState job,
        string fileName,
        byte[] fileBytes,
        CancellationToken ct = default)
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"job_{job.Id}");
        Directory.CreateDirectory(outputPath);
        var filePath = Path.Combine(outputPath, fileName);
        await File.WriteAllBytesAsync(filePath, fileBytes, ct);

        job.SetResultFileName(fileName);
        job.MarkCompleted(
            resultUrl: $"/api/jobs/{job.Id}/download",
            resultFileName: fileName,
            resultFileSizeBytes: fileBytes.Length);
    }

    public static string GetJobOutputDirectory(Guid jobId)
    {
        return Path.Combine(Path.GetTempPath(), $"job_{jobId}");
    }

    public static void CleanupJobDirectory(Guid jobId)
    {
        var outputPath = GetJobOutputDirectory(jobId);
        if (Directory.Exists(outputPath))
        {
            try
            {
                Directory.Delete(outputPath, recursive: true);
            }
            catch
            {
            }
        }
    }
}

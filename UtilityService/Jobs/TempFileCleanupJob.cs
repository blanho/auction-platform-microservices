using Common.Scheduling.Jobs;
using Common.Storage.Abstractions;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UtilityService.Interfaces;

namespace UtilityService.Jobs;

public class TempFileCleanupJob : BaseJob
{
    public const string JobId = "temp-file-cleanup";
    public const string Description = "Cleans up expired temporary files";

    public TempFileCleanupJob(
        ILogger<TempFileCleanupJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();
        var storageProvider = scopedProvider.GetRequiredService<IStorageProvider>();
        var options = scopedProvider.GetRequiredService<IOptions<StorageOptions>>().Value;

        var expirationThreshold = DateTime.UtcNow.AddHours(-options.TempFileExpirationHours);
        var expiredFiles = await unitOfWork.StoredFiles.GetTemporaryFilesAsync(
            expirationThreshold, cancellationToken);

        var expiredFilesList = expiredFiles.ToList();

        if (expiredFilesList.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} expired temp files to clean up", expiredFilesList.Count);
        var cleanedCount = 0;

        foreach (var file in expiredFilesList)
        {
            try
            {
                await storageProvider.DeleteAsync(file.Path, cancellationToken);

                file.Status = FileStatus.Deleted;
                file.DeletedAt = DateTime.UtcNow;

                cleanedCount++;
                Logger.LogDebug("Cleaned up expired temp file: {FileId} - {FileName}", file.Id, file.FileName);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to clean up temp file: {FileId}", file.Id);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Cleaned up {Count} expired temp files", cleanedCount);
    }
}

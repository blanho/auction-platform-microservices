using BuildingBlocks.Application.Abstractions.Storage;
using BuildingBlocks.Infrastructure.Scheduling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Storage.Application.Interfaces;
using Storage.Infrastructure.Persistence;

namespace Storage.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class OrphanFileCleanupJob : BaseJob
{
    public const string JobId = "orphan-file-cleanup";
    public const string Description = "Purges soft-deleted file records and cleans up unassociated files";

    private static readonly TimeSpan SoftDeleteRetention = TimeSpan.FromDays(StorageDefaults.Cleanup.SoftDeleteRetentionDays);
    private static readonly TimeSpan UnassociatedFileThreshold = TimeSpan.FromHours(StorageDefaults.Cleanup.UnassociatedFileThresholdHours);

    public OrphanFileCleanupJob(
        ILogger<OrphanFileCleanupJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var repository = scopedProvider.GetRequiredService<IStoredFileRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();
        var fileStorageService = scopedProvider.GetRequiredService<IFileStorageService>();
        var dbContext = scopedProvider.GetRequiredService<StorageDbContext>();

        await PurgeSoftDeletedRecordsAsync(repository, unitOfWork, dbContext, cancellationToken);
        await CleanupUnassociatedFilesAsync(repository, unitOfWork, fileStorageService, dbContext, cancellationToken);
    }

    private async Task PurgeSoftDeletedRecordsAsync(
        IStoredFileRepository repository,
        IUnitOfWork unitOfWork,
        StorageDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var threshold = DateTimeOffset.UtcNow - SoftDeleteRetention;
        var totalPurged = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await repository.GetSoftDeletedOlderThanAsync(threshold, StorageDefaults.Cleanup.BatchSize, cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            repository.RemoveRange(batch);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();

            totalPurged += batch.Count;

            Logger.LogDebug("Purged {Count} soft-deleted file records", batch.Count);
        }

        if (totalPurged > 0)
        {
            Logger.LogInformation("Purged {TotalCount} soft-deleted file records older than {Retention} days",
                totalPurged, SoftDeleteRetention.TotalDays);
        }
    }

    private async Task CleanupUnassociatedFilesAsync(
        IStoredFileRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        StorageDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var threshold = DateTimeOffset.UtcNow - UnassociatedFileThreshold;
        var totalCleaned = 0;
        var failedDeletes = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await repository.GetUnassociatedOlderThanAsync(threshold, StorageDefaults.Cleanup.BatchSize, cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            foreach (var file in batch)
            {
                try
                {
                    var deleted = await fileStorageService.DeleteAsync(file.StoredFileName, cancellationToken);

                    if (!deleted)
                    {
                        Logger.LogWarning("Physical file not found for orphan cleanup: {StoredFileName}", file.StoredFileName);
                    }

                    file.MarkAsDeleted(null);
                }
                catch (Exception ex)
                {
                    failedDeletes++;
                    Logger.LogError(ex, "Failed to delete orphan file {FileId} ({StoredFileName})",
                        file.Id, file.StoredFileName);
                }
            }

            repository.RemoveRange(batch.Where(f => f.IsDeleted));
            await unitOfWork.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();

            totalCleaned += batch.Count(f => f.IsDeleted);
        }

        if (totalCleaned > 0 || failedDeletes > 0)
        {
            Logger.LogInformation(
                "Orphan cleanup: {CleanedCount} files cleaned, {FailedCount} failures",
                totalCleaned, failedDeletes);
        }
    }
}

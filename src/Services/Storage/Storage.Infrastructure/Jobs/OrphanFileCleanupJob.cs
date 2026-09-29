using BuildingBlocks.Application.Abstractions;
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

        await PurgeSoftDeletedRecordsAsync(repository, unitOfWork, fileStorageService, dbContext, cancellationToken);
        await CleanupUnassociatedFilesAsync(repository, unitOfWork, dbContext, cancellationToken);
        await PurgeSoftDeletedRecordsAsync(repository, unitOfWork, fileStorageService, dbContext, cancellationToken);
    }

    private async Task PurgeSoftDeletedRecordsAsync(
        IStoredFileRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        StorageDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var threshold = DateTimeOffset.UtcNow;
        var totalPurged = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await repository.GetSoftDeletedOlderThanAsync(threshold, StorageDefaults.Cleanup.BatchSize, cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            var purged = new List<Domain.Entities.StoredFile>();
            foreach (var file in batch)
            {
                try
                {
                    await fileStorageService.DeleteAsync(file.StoredFileName, cancellationToken);
                    if (!await fileStorageService.ExistsAsync(file.StoredFileName, cancellationToken))
                        purged.Add(file);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Logger.LogWarning(ex, "Will retry physical deletion for {FileId}", file.Id);
                }
            }
            if (purged.Count == 0) break;
            repository.RemoveRange(purged);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();

            totalPurged += purged.Count;

            Logger.LogDebug("Purged {Count} soft-deleted file records", batch.Count);
        }

        if (totalPurged > 0)
        {
            Logger.LogInformation("Purged {TotalCount} soft-deleted file records after physical deletion",
                totalPurged);
        }
    }

    private async Task CleanupUnassociatedFilesAsync(
        IStoredFileRepository repository,
        IUnitOfWork unitOfWork,
        StorageDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var threshold = DateTimeOffset.UtcNow - UnassociatedFileThreshold;
        while (!cancellationToken.IsCancellationRequested)
        {
            var batch = await repository.GetUnassociatedOlderThanAsync(threshold, StorageDefaults.Cleanup.BatchSize, cancellationToken);
            if (batch.Count == 0) break;

            foreach (var file in batch)
            {
                file.MarkAsDeleted(null);
                repository.Update(file);
            }
            await unitOfWork.SaveChangesAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            Logger.LogDebug("Marked {Count} unassociated files for physical deletion", batch.Count);
        }
    }
}

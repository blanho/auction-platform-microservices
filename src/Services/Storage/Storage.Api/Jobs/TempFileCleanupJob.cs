using Microsoft.Extensions.Options;
using Quartz;
using Storage.Application.Configuration;
using Storage.Application.Interfaces;

namespace Storage.Api.Jobs;

[DisallowConcurrentExecution]
public class TempFileCleanupJob : IJob
{
    public const string JobId = "temp-file-cleanup";
    public const string Description = "Cleans up expired temporary files and permanently deletes soft-deleted files from storage";

    private readonly Storage.Application.Interfaces.IUnitOfWork _unitOfWork;
    private readonly IStorageProvider _storageProvider;
    private readonly StorageOptions _options;
    private readonly ILogger<TempFileCleanupJob> _logger;

    public TempFileCleanupJob(
        Storage.Application.Interfaces.IUnitOfWork unitOfWork,
        IStorageProvider storageProvider,
        IOptions<StorageOptions> options,
        ILogger<TempFileCleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _storageProvider = storageProvider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting file cleanup job");

        await CleanupExpiredFilesAsync(context.CancellationToken);
        await PurgeDeletedFilesAsync(context.CancellationToken);

        _logger.LogInformation("File cleanup job completed");
    }

    private async Task CleanupExpiredFilesAsync(CancellationToken cancellationToken)
    {
        var expiredFiles = await _unitOfWork.StoredFiles.GetExpiredFilesAsync(DateTimeOffset.UtcNow, cancellationToken);
        var fileList = expiredFiles.ToList();

        if (fileList.Count == 0)
        {
            _logger.LogInformation("No expired files found");
            return;
        }

        _logger.LogInformation("Found {Count} expired files to clean up", fileList.Count);

        var deletedCount = 0;
        foreach (var file in fileList)
        {
            try
            {
                await _storageProvider.DeleteAsync(file.StoragePath, cancellationToken);
                file.MarkRemoved();
                _unitOfWork.StoredFiles.Update(file);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete expired file {FileId}: {Path}", file.Id, file.StoragePath);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Expired file cleanup completed. Deleted {DeletedCount} of {TotalCount} files", deletedCount, fileList.Count);
    }

    private async Task PurgeDeletedFilesAsync(CancellationToken cancellationToken)
    {
        var deletedFiles = await _unitOfWork.StoredFiles.GetDeletedFilesAsync(100, cancellationToken);
        var fileList = deletedFiles.ToList();

        if (fileList.Count == 0)
        {
            _logger.LogInformation("No soft-deleted files to purge from storage");
            return;
        }

        _logger.LogInformation("Found {Count} soft-deleted files to purge from storage", fileList.Count);

        var purgedCount = 0;
        foreach (var file in fileList)
        {
            try
            {
                var deleted = await _storageProvider.DeleteAsync(file.StoragePath, cancellationToken);
                if (deleted)
                {
                    _unitOfWork.StoredFiles.Delete(file);
                    purgedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to purge file {FileId} from storage: {Path}", file.Id, file.StoragePath);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Storage purge completed. Purged {PurgedCount} of {TotalCount} files", purgedCount, fileList.Count);
    }
}

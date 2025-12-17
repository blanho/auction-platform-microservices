using Microsoft.Extensions.Options;
using Quartz;
using StorageService.Application.Configuration;
using StorageService.Application.Interfaces;
using StorageService.Domain.Entities;

namespace StorageService.API.Jobs;

[DisallowConcurrentExecution]
public class TempFileCleanupJob : IJob
{
    public const string JobId = "temp-file-cleanup";
    public const string Description = "Cleans up temporary files that were not confirmed within the expiration period";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageProvider _storageProvider;
    private readonly StorageOptions _options;
    private readonly ILogger<TempFileCleanupJob> _logger;

    public TempFileCleanupJob(
        IUnitOfWork unitOfWork,
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
        _logger.LogInformation("Starting temporary file cleanup job");

        var cutoffTime = DateTimeOffset.UtcNow.AddHours(-_options.TempFileExpirationHours);
        var expiredFiles = await _unitOfWork.StoredFiles.GetTemporaryFilesOlderThanAsync(cutoffTime, context.CancellationToken);
        var fileList = expiredFiles.ToList();

        if (fileList.Count == 0)
        {
            _logger.LogInformation("No expired temporary files found");
            return;
        }

        _logger.LogInformation("Found {Count} expired temporary files to clean up", fileList.Count);

        var deletedCount = 0;
        foreach (var file in fileList)
        {
            try
            {
                await _storageProvider.DeleteAsync(file.Path, context.CancellationToken);
                file.Status = FileStatus.Deleted;
                file.DeletedAt = DateTimeOffset.UtcNow;
                _unitOfWork.StoredFiles.Update(file);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete temporary file {FileId}: {Path}", file.Id, file.Path);
            }
        }

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        _logger.LogInformation("Temporary file cleanup completed. Deleted {DeletedCount} of {TotalCount} files", deletedCount, fileList.Count);
    }
}

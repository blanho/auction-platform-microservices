using Microsoft.Extensions.Options;
using Quartz;
using Storage.Application.Configuration;
using Storage.Application.Interfaces;
using Storage.Domain.Enums;
using Storage.Domain.ValueObjects;
using IUnitOfWork = Storage.Application.Interfaces.IUnitOfWork;

namespace Storage.Api.Jobs;

[DisallowConcurrentExecution]
public class FileReconciliationJob : IJob
{
    public const string JobId = "file-reconciliation-job";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageProvider _storageProvider;
    private readonly IVirusScanService? _virusScanService;
    private readonly StorageOptions _options;
    private readonly ILogger<FileReconciliationJob> _logger;

    public FileReconciliationJob(
        IUnitOfWork unitOfWork,
        IStorageProvider storageProvider,
        IOptions<StorageOptions> options,
        ILogger<FileReconciliationJob> logger,
        IVirusScanService? virusScanService = null)
    {
        _unitOfWork = unitOfWork;
        _storageProvider = storageProvider;
        _options = options.Value;
        _logger = logger;
        _virusScanService = virusScanService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        _logger.LogInformation("Starting file reconciliation job");

        try
        {

            await HandleStaleScanningFilesAsync(cancellationToken);

            await CleanupTempFilesAsync(cancellationToken);

            await CleanupQuarantineAsync(cancellationToken);

            await PurgeSoftDeletedFilesAsync(cancellationToken);

            if (_options.Lifecycle.EnableReconciliation)
            {
                await ReconcileStorageAsync(cancellationToken);
            }

            _logger.LogInformation("File reconciliation job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File reconciliation job failed");
            throw;
        }
    }

    private async Task HandleStaleScanningFilesAsync(CancellationToken cancellationToken)
    {
        var scanTimeout = TimeSpan.FromSeconds(_options.Scanning.TimeoutSeconds);
        var staleFiles = await _unitOfWork.StoredFiles.GetPendingScanFilesAsync(
            scanTimeout,
            _options.Lifecycle.ReconciliationBatchSize,
            cancellationToken);

        var count = 0;
        foreach (var file in staleFiles)
        {
            try
            {

                if (_virusScanService != null)
                {
                    var scanStatus = await _virusScanService.CheckScanStatusAsync(
                        file.StoragePath,
                        cancellationToken);

                    if (scanStatus == ScanStatus.Complete)
                    {
                        var result = await _virusScanService.ScanFileAsync(
                            file.StoragePath,
                            cancellationToken);

                        if (result.IsInfected)
                        {
                            file.MarkInfected(result);
                            var quarantinePath = await _virusScanService.QuarantineFileAsync(
                                file.StoragePath,
                                cancellationToken);
                            file.SetQuarantinePath(quarantinePath);
                        }
                        else
                        {
                            file.MarkScanned(result);
                        }
                    }
                    else if (scanStatus == ScanStatus.Failed)
                    {
                        file.MarkScanned(ScanResult.Error("Scan timeout", "ReconciliationJob"));
                    }
                    else
                    {

                        await _virusScanService.TriggerScanAsync(file.StoragePath, cancellationToken);
                    }
                }
                else
                {

                    file.MarkScanned(ScanResult.Clean("NoScan", "Scanner not configured"));
                }

                _unitOfWork.StoredFiles.Update(file);
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to handle stale scanning file {FileId}", file.Id);
            }
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Handled {Count} stale scanning files", count);
        }
    }

    private async Task CleanupTempFilesAsync(CancellationToken cancellationToken)
    {
        var maxAge = TimeSpan.FromHours(_options.Lifecycle.TempFileMaxAgeHours);
        var tempFiles = await _unitOfWork.StoredFiles.GetTempFilesForCleanupAsync(
            maxAge,
            _options.Lifecycle.ReconciliationBatchSize,
            cancellationToken);

        var count = 0;
        foreach (var file in tempFiles)
        {
            try
            {

                var deleted = await _storageProvider.DeleteAsync(file.StoragePath, cancellationToken);

                file.MarkExpired("Temp file expired");
                _unitOfWork.StoredFiles.Update(file);
                count++;

                if (!deleted)
                {
                    _logger.LogWarning(
                        "File {FileId} marked expired but storage deletion failed: {Path}",
                        file.Id, file.StoragePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temp file {FileId}", file.Id);
            }
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} expired temp files", count);
        }
    }

    private async Task CleanupQuarantineAsync(CancellationToken cancellationToken)
    {
        var retentionPeriod = TimeSpan.FromDays(_options.Lifecycle.QuarantineRetentionDays);
        var cutoff = DateTimeOffset.UtcNow - retentionPeriod;

        var infectedFiles = await _unitOfWork.StoredFiles.GetInfectedFilesAsync(
            _options.Lifecycle.ReconciliationBatchSize,
            cancellationToken);

        var count = 0;
        foreach (var file in infectedFiles.Where(f => f.UpdatedAt < cutoff))
        {
            try
            {

                if (file.ExtendedProperties?.TempPath != null)
                {
                    await _storageProvider.DeleteAsync(file.ExtendedProperties.TempPath, cancellationToken);
                }

                file.MarkRemoved("Quarantine retention expired");
                _unitOfWork.StoredFiles.Update(file);
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup quarantined file {FileId}", file.Id);
            }
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} quarantined files", count);
        }
    }

    private async Task PurgeSoftDeletedFilesAsync(CancellationToken cancellationToken)
    {
        var retentionPeriod = TimeSpan.FromDays(_options.Lifecycle.SoftDeleteRetentionDays);
        var cutoff = DateTimeOffset.UtcNow - retentionPeriod;

        var deletedFiles = await _unitOfWork.StoredFiles.GetDeletedFilesAsync(
            _options.Lifecycle.ReconciliationBatchSize,
            cancellationToken);

        var count = 0;
        foreach (var file in deletedFiles.Where(f => f.DeletedAt.HasValue && f.DeletedAt < cutoff))
        {
            try
            {

                await _storageProvider.DeleteAsync(file.StoragePath, cancellationToken);

                _unitOfWork.StoredFiles.Delete(file);
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to purge deleted file {FileId}", file.Id);
            }
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Purged {Count} soft-deleted files", count);
        }
    }

    private async Task ReconcileStorageAsync(CancellationToken cancellationToken)
    {

        var confirmedFiles = await _unitOfWork.StoredFiles.GetByStatusAsync(
            FileStatus.Confirmed,
            cancellationToken);

        var count = 0;
        foreach (var file in confirmedFiles.Take(_options.Lifecycle.ReconciliationBatchSize))
        {
            try
            {
                var exists = await _storageProvider.ExistsAsync(file.StoragePath, cancellationToken);

                if (!exists)
                {
                    _logger.LogWarning(
                        "File {FileId} exists in database but not in storage: {Path}",
                        file.Id, file.StoragePath);

                    file.MarkRemoved("Storage file not found during reconciliation");
                    _unitOfWork.StoredFiles.Update(file);
                    count++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to reconcile file {FileId}", file.Id);
            }
        }

        if (count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Found {Count} orphaned database records", count);
        }
    }
}

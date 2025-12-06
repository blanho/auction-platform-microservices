using Common.Storage.Abstractions;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UtilityService.Data;

namespace UtilityService.BackgroundServices;

public class TempFileCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TempFileCleanupService> _logger;
    private readonly StorageOptions _options;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public TempFileCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TempFileCleanupService> logger,
        IOptions<StorageOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Temp file cleanup service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredTempFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temp file cleanup");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupExpiredTempFilesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UtilityDbContext>();
        var storageProvider = scope.ServiceProvider.GetRequiredService<IStorageProvider>();

        var expirationThreshold = DateTime.UtcNow.AddHours(-_options.TempFileExpirationHours);

        var expiredFiles = await dbContext.StoredFiles
            .Where(f => f.Status == FileStatus.Temporary && f.CreatedAt < expirationThreshold)
            .ToListAsync(cancellationToken);

        if (expiredFiles.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} expired temp files to clean up", expiredFiles.Count);

        foreach (var file in expiredFiles)
        {
            try
            {
                await storageProvider.DeleteAsync(file.Path, cancellationToken);
                
                file.Status = FileStatus.Deleted;
                file.DeletedAt = DateTime.UtcNow;
                
                _logger.LogDebug("Cleaned up expired temp file: {FileId} - {FileName}", file.Id, file.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temp file: {FileId}", file.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cleaned up {Count} expired temp files", expiredFiles.Count);
    }
}

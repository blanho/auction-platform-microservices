using BuildingBlocks.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Interfaces;
using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using Storage.Infrastructure.Persistence;

namespace Storage.Infrastructure.Repositories;

public class StoredFileRepository : IStoredFileRepository
{
    private readonly StorageDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public StoredFileRepository(StorageDbContext context, IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<StoredFile?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.OwnerService == ownerService && f.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetExpiredFilesAsync(
        DateTimeOffset expiredBefore,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.ExpiresAt != null && f.ExpiresAt < expiredBefore && f.Status != FileStatus.Removed)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetDeletedFilesAsync(
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.Status == FileStatus.Removed)
            .OrderBy(f => f.DeletedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByResourceAsync(
        Guid resourceId,
        StorageResourceType resourceType,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.ResourceId == resourceId 
                && f.ResourceType == resourceType 
                && f.Status != FileStatus.Removed
                && f.Status != FileStatus.Infected)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetPendingScanFilesAsync(
        TimeSpan olderThan,
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default)
    {
        var cutoff = _dateTime.UtcNowOffset - olderThan;
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.Status == FileStatus.Scanning && f.UpdatedAt < cutoff)
            .OrderBy(f => f.UpdatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetInfectedFilesAsync(
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.Status == FileStatus.Infected)
            .OrderBy(f => f.UpdatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetTempFilesForCleanupAsync(
        TimeSpan olderThan,
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default)
    {
        var cutoff = _dateTime.UtcNowOffset - olderThan;
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => (f.Status == FileStatus.InTemp || f.Status == FileStatus.Pending) 
                && f.CreatedAt < cutoff)
            .OrderBy(f => f.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<StoredFile?> GetByContentHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.ContentHash == contentHash 
                && (f.Status == FileStatus.InMedia || f.Status == FileStatus.Confirmed))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<FileStatus, int>> GetFileCountsByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .GroupBy(f => f.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
    }

    public async Task<bool> ExistsByStoragePathAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .AnyAsync(f => f.StoragePath == storagePath, cancellationToken);
    }

    public async Task AddAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        await _context.StoredFiles.AddAsync(file, cancellationToken);
    }

    public void Update(StoredFile file)
    {
        _context.StoredFiles.Update(file);
    }

    public void Delete(StoredFile file)
    {
        _context.StoredFiles.Remove(file);
    }
}

using Microsoft.EntityFrameworkCore;
using StorageService.Application.Interfaces;
using StorageService.Domain.Constants;
using StorageService.Domain.Entities;
using StorageService.Domain.Enums;
using StorageService.Infrastructure.Data;

namespace StorageService.Infrastructure.Repositories;

public class StoredFileRepository : IStoredFileRepository
{
    private readonly StorageDbContext _context;

    public StoredFileRepository(StorageDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(f => f.OwnerService == ownerService && f.OwnerId == ownerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetExpiredFilesAsync(
        DateTimeOffset expiredBefore,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(f => f.ExpiresAt != null && f.ExpiresAt < expiredBefore && f.Status != FileStatus.Deleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(f => f.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetDeletedFilesAsync(
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(f => f.Status == FileStatus.Deleted)
            .OrderBy(f => f.DeletedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
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


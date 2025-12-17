using Common.Storage.Enums;
using Microsoft.EntityFrameworkCore;
using UtilityService.Data;
using UtilityService.Domain.Entities;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class StoredFileRepository : IStoredFileRepository
{
    private readonly UtilityDbContext _context;

    public StoredFileRepository(UtilityDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByOwnerEntityAsync(
        string ownerService,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(x => x.OwnerService == ownerService && x.EntityId == entityId && x.Status != FileStatus.Deleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetTemporaryFilesAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(x => x.Status == FileStatus.Temporary && x.CreatedAt < olderThan)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StoredFile>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.StoredFiles
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        await _context.StoredFiles.AddAsync(file, cancellationToken);
    }

    public Task UpdateAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        _context.StoredFiles.Update(file);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(StoredFile file, CancellationToken cancellationToken = default)
    {
        _context.StoredFiles.Remove(file);
        return Task.CompletedTask;
    }
}

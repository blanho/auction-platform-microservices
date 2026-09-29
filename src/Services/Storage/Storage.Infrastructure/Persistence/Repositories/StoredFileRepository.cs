using Microsoft.EntityFrameworkCore;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Infrastructure.Persistence.Repositories;

public class StoredFileRepository : IStoredFileRepository
{
    private readonly StorageDbContext _context;

    public StoredFileRepository(StorageDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.StoredFiles.FindAsync([id], ct);
    }

    public Task<StoredFile?> GetByReportRequestIdAsync(Guid ownerId, Guid requestId, CancellationToken ct = default) =>
        _context.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.OwnerId == ownerId && f.ReportRequestId == requestId, ct);

    public Task<StoredFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken ct = default) =>
        _context.StoredFiles.SingleOrDefaultAsync(f => f.StoredFileName == storedFileName, ct);

    public async Task<List<StoredFile>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.OwnerId == ownerId && f.Status == FileStatus.Ready)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(StoredFile file, CancellationToken ct = default)
    {
        await _context.StoredFiles.AddAsync(file, ct);
    }

    public async Task AddRangeAsync(IEnumerable<StoredFile> files, CancellationToken ct = default)
    {
        await _context.StoredFiles.AddRangeAsync(files, ct);
    }

    public void Update(StoredFile file)
    {
        _context.StoredFiles.Update(file);
    }

    public async Task<List<StoredFile>> GetSoftDeletedOlderThanAsync(
        DateTimeOffset threshold, int batchSize, CancellationToken ct = default)
    {
        return await _context.StoredFiles
            .IgnoreQueryFilters()
            .Where(f => f.IsDeleted && f.DeletedAt != null && f.DeletedAt < threshold)
            .OrderBy(f => f.DeletedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<List<StoredFile>> GetUnassociatedOlderThanAsync(
        DateTimeOffset threshold, int batchSize, CancellationToken ct = default)
    {
        return await _context.StoredFiles
            .Where(f => (f.OwnerId == null && f.Status == FileStatus.Ready && f.CreatedAt < threshold)
                || (f.Status == FileStatus.Pending && f.UploadExpiresAt < threshold))
            .OrderBy(f => f.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void RemoveRange(IEnumerable<StoredFile> files)
    {
        _context.StoredFiles.RemoveRange(files);
    }
}

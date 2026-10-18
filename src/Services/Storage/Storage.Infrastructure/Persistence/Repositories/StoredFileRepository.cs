using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Infrastructure.Persistence.Repositories;

public class StoredFileRepository : EfRepository<StoredFile, StorageDbContext>, IStoredFileRepository
{
    public StoredFileRepository(
        StorageDbContext context, IDateTimeProvider clock, IAuditContext auditContext)
        : base(context, clock, auditContext)
    {
    }

    public override Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetByIdForUpdateAsync(id, cancellationToken);

    public Task<StoredFile?> GetByReportRequestIdAsync(Guid ownerId, Guid requestId, CancellationToken ct = default) =>
        Context.StoredFiles.AsNoTracking().SingleOrDefaultAsync(f => f.OwnerId == ownerId && f.ReportRequestId == requestId, ct);

    public Task<StoredFile?> GetByStoredFileNameAsync(string storedFileName, CancellationToken ct = default) =>
        Context.StoredFiles.SingleOrDefaultAsync(f => f.StoredFileName == storedFileName, ct);

    public async Task<List<StoredFile>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await Context.StoredFiles
            .AsNoTracking()
            .Where(f => f.OwnerId == ownerId && f.Status == FileStatus.Ready)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);
    }

    Task IStoredFileRepository.AddAsync(StoredFile file, CancellationToken ct) => AddAsync(file, ct);

    protected override void ApplyDeletedAudit(StoredFile entity, DateTimeOffset timestamp)
    {
        entity.MarkAsDeleted(GetDeletedBy(entity));
        base.ApplyDeletedAudit(entity, timestamp);
    }

    public async Task<List<StoredFile>> GetSoftDeletedOlderThanAsync(
        DateTimeOffset threshold, int batchSize, CancellationToken ct = default)
    {
        return await Context.StoredFiles
            .IgnoreQueryFilters()
            .Where(f => f.IsDeleted && f.DeletedAt != null && f.DeletedAt < threshold)
            .OrderBy(f => f.DeletedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task<List<StoredFile>> GetUnassociatedOlderThanAsync(
        DateTimeOffset threshold, int batchSize, CancellationToken ct = default)
    {
        return await Context.StoredFiles
            .Where(f => (f.OwnerId == null && f.Status == FileStatus.Ready && f.CreatedAt < threshold)
                || (f.Status == FileStatus.Pending && f.UploadExpiresAt < threshold))
            .OrderBy(f => f.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public void RemoveRange(IEnumerable<StoredFile> files)
    {
        Context.StoredFiles.RemoveRange(files);
    }
}

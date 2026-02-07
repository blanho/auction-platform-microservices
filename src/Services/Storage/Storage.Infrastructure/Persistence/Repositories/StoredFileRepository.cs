using Microsoft.EntityFrameworkCore;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;

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

    public async Task<List<StoredFile>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await _context.StoredFiles
            .AsNoTracking()
            .Where(f => f.OwnerId == ownerId)
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
}

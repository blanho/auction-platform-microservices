using StorageService.Application.Interfaces;
using StorageService.Infrastructure.Data;

namespace StorageService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly StorageDbContext _context;
    private IStoredFileRepository _storedFiles;

    public UnitOfWork(StorageDbContext context)
    {
        _context = context;
    }

    public IStoredFileRepository StoredFiles => _storedFiles ??= new StoredFileRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

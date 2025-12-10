using Microsoft.EntityFrameworkCore.Storage;
using UtilityService.Data;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly UtilityDbContext _context;
    private IDbContextTransaction? _transaction;
    private IAuditLogRepository? _auditLogs;
    private IStoredFileRepository? _storedFiles;

    public UnitOfWork(UtilityDbContext context)
    {
        _context = context;
    }

    public IAuditLogRepository AuditLogs
    {
        get { return _auditLogs ??= new AuditLogRepository(_context); }
    }

    public IStoredFileRepository StoredFiles
    {
        get { return _storedFiles ??= new StoredFileRepository(_context); }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

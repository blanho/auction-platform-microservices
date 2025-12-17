using StorageService.Domain.Entities;
using StorageService.Domain.Enums;

namespace StorageService.Application.Interfaces;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByOwnerAsync(
        string ownerService,
        string ownerId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetExpiredFilesAsync(
        DateTimeOffset expiredBefore,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetDeletedFilesAsync(
        int batchSize = 100,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);
    
    void Update(StoredFile file);
    
    void Delete(StoredFile file);
}

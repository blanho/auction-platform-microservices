using StorageService.Domain.Entities;

namespace StorageService.Application.Interfaces;

public interface IStoredFileRepository
{
    Task<StoredFile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetTemporaryFilesOlderThanAsync(
        DateTimeOffset olderThan,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);
    
    void Update(StoredFile file);
    
    void Delete(StoredFile file);
}

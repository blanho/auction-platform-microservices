using Common.Storage.Enums;
using UtilityService.Domain.Entities;

namespace UtilityService.Interfaces;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByEntityAsync(
        string entityType,
        string entityId,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetTemporaryFilesAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<StoredFile>> GetByStatusAsync(
        FileStatus status,
        CancellationToken cancellationToken = default);
    
    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(StoredFile file, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(StoredFile file, CancellationToken cancellationToken = default);
}

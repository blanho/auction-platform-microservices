using Storage.Domain.Constants;
using Storage.Domain.Entities;
using Storage.Domain.Enums;

namespace Storage.Application.Interfaces;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

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
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StoredFile>> GetByResourceAsync(
        Guid resourceId,
        StorageResourceType resourceType,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StoredFile>> GetPendingScanFilesAsync(
        TimeSpan olderThan,
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StoredFile>> GetInfectedFilesAsync(
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StoredFile>> GetTempFilesForCleanupAsync(
        TimeSpan olderThan,
        int batchSize = StorageDefaults.MaxBatchSize,
        CancellationToken cancellationToken = default);

    Task<StoredFile?> GetByContentHashAsync(
        string contentHash,
        CancellationToken cancellationToken = default);

    Task<Dictionary<FileStatus, int>> GetFileCountsByStatusAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByStoragePathAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task AddAsync(StoredFile file, CancellationToken cancellationToken = default);

    void Update(StoredFile file);

    void Delete(StoredFile file);
}

using Storage.Domain.Entities;

namespace Storage.Application.Interfaces;

public interface IStoredFileRepository
{
    Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<StoredFile>> GetByOwnerIdAsync(Guid ownerId, CancellationToken ct = default);
    Task AddAsync(StoredFile file, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<StoredFile> files, CancellationToken ct = default);
    void Update(StoredFile file);
}

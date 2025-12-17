namespace StorageService.Application.Interfaces;

public interface IUnitOfWork
{
    IStoredFileRepository StoredFiles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

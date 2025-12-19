using Common.Repository.Interfaces;

namespace StorageService.Application.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IStoredFileRepository StoredFiles { get; }
}

namespace Storage.Application.Interfaces;

public interface IUnitOfWork : BuildingBlocks.Infrastructure.Repository.IUnitOfWork
{
    IStoredFileRepository StoredFiles { get; }
}

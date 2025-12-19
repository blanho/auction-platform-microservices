using Common.Repository.Implementations;
using MediatR;
using StorageService.Application.Interfaces;
using StorageService.Infrastructure.Data;

namespace StorageService.Infrastructure.Repositories;

public class UnitOfWork : BaseUnitOfWork<StorageDbContext>, IUnitOfWork
{
    private IStoredFileRepository? _storedFiles;

    public UnitOfWork(StorageDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }

    public IStoredFileRepository StoredFiles => _storedFiles ??= new StoredFileRepository(Context);
}

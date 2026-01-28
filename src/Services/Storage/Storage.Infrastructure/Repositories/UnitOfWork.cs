using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using MediatR;
using Storage.Application.Interfaces;
using Storage.Infrastructure.Persistence;

namespace Storage.Infrastructure.Repositories;

public class UnitOfWork : BaseUnitOfWork<StorageDbContext>, global::Storage.Application.Interfaces.IUnitOfWork
{
    private IStoredFileRepository? _storedFiles;
    private readonly IDateTimeProvider _dateTime;

    public UnitOfWork(StorageDbContext context, IMediator mediator, IDateTimeProvider dateTime, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
        _dateTime = dateTime;
    }

    public IStoredFileRepository StoredFiles => _storedFiles ??= new StoredFileRepository(Context, _dateTime);
}

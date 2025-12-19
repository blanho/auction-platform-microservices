using Common.Repository.Implementations;
using MediatR;
using UtilityService.Data;
using UtilityService.Interfaces;

namespace UtilityService.Repositories;

public class UnitOfWork : BaseUnitOfWork<UtilityDbContext>, IUnitOfWork
{
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(UtilityDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(Context);
}

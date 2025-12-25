using Common.Repository.Implementations;
using MediatR;
using AnalyticsService.Data;
using AnalyticsService.Interfaces;

namespace AnalyticsService.Repositories;

public class UnitOfWork : BaseUnitOfWork<AnalyticsDbContext>, IUnitOfWork
{
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AnalyticsDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(Context);
}

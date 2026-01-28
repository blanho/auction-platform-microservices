using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using Analytics.Api.Data;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Repositories;

public class UnitOfWork : BaseUnitOfWork<AnalyticsDbContext>, Analytics.Api.Interfaces.IUnitOfWork
{
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AnalyticsDbContext context, IMediator mediator, IAuditPublisher? auditPublisher = null)
        : base(context, mediator, auditPublisher)
    {
    }

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(Context);
}

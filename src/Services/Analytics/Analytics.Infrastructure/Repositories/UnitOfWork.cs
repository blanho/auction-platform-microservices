using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using BuildingBlocks.Infrastructure.Repository;
using MediatR;
using Analytics.Infrastructure.Persistence;
using Analytics.Application.Interfaces;

namespace Analytics.Infrastructure.Repositories;

public class UnitOfWork : BaseUnitOfWork<AnalyticsDbContext>, IUnitOfWork
{
    private IAuditLogRepository? _auditLogs;

    public UnitOfWork(AnalyticsDbContext context, IMediator mediator)
        : base(context, mediator)
    {
    }

    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(Context);
}

using BuildingBlocks.Infrastructure.Repository;

namespace Analytics.Api.Interfaces;

public interface IUnitOfWork : BuildingBlocks.Infrastructure.Repository.IUnitOfWork
{
    IAuditLogRepository AuditLogs { get; }
}

using Common.Repository.Interfaces;

namespace AnalyticsService.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IAuditLogRepository AuditLogs { get; }
}

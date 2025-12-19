using Common.Repository.Interfaces;

namespace UtilityService.Interfaces;

public interface IUnitOfWork : IBaseUnitOfWork
{
    IAuditLogRepository AuditLogs { get; }
}

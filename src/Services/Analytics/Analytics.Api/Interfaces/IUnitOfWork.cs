using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Interfaces;

public interface IUnitOfWork : BuildingBlocks.Application.Abstractions.IUnitOfWork
{
    IAuditLogRepository AuditLogs { get; }
}

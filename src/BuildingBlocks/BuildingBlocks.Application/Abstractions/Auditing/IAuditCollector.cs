namespace BuildingBlocks.Application.Abstractions.Auditing;

public interface IAuditCollector
{
    IReadOnlyList<AuditEntry> GetPendingAudits();
    void Clear();
}

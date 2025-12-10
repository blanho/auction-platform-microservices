using Common.Audit.Abstractions;

namespace Common.Audit.Implementations;

public class ScopedAuditContext : IAuditContext
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
}

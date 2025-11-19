using Common.Contracts.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics.Api.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid EntityId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? OldValues { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? NewValues { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? ChangedProperties { get; set; }
    
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

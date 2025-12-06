using System.Text.Json;
using Common.Audit.Events;
using MassTransit;
using UtilityService.Data;
using UtilityService.Domain.Entities;

namespace UtilityService.Consumers;
public class AuditEventConsumer : IConsumer<AuditEvent>
{
    private readonly UtilityDbContext _context;
    private readonly ILogger<AuditEventConsumer> _logger;

    public AuditEventConsumer(UtilityDbContext context, ILogger<AuditEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditEvent> context)
    {
        var auditEvent = context.Message;

        _logger.LogInformation(
            "Received audit event: {Action} on {EntityType} ({EntityId}) from {ServiceName}",
            auditEvent.Action,
            auditEvent.EntityType,
            auditEvent.EntityId,
            auditEvent.ServiceName);

        var auditLog = new AuditLog
        {
            Id = auditEvent.Id,
            EntityId = auditEvent.EntityId,
            EntityType = auditEvent.EntityType,
            Action = auditEvent.Action,
            OldValues = auditEvent.OldValues,
            NewValues = auditEvent.NewValues,
            ChangedProperties = auditEvent.ChangedProperties.Count > 0 
                ? JsonSerializer.Serialize(auditEvent.ChangedProperties) 
                : null,
            UserId = auditEvent.UserId,
            Username = auditEvent.Username,
            ServiceName = auditEvent.ServiceName,
            CorrelationId = auditEvent.CorrelationId,
            IpAddress = auditEvent.IpAddress,
            Timestamp = auditEvent.Timestamp,
            Metadata = auditEvent.Metadata != null 
                ? JsonSerializer.Serialize(auditEvent.Metadata) 
                : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Audit log {Id} saved successfully", auditLog.Id);
    }
}

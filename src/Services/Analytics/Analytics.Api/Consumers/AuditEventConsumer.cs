using System.Text.Json;
using Common.Contracts.Events;
using MassTransit;
using Analytics.Api.Entities;
using Analytics.Api.Interfaces;

namespace Analytics.Api.Consumers;

public class AuditEventConsumer : IConsumer<AuditEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditEventConsumer> _logger;

    public AuditEventConsumer(
        IUnitOfWork unitOfWork,
        ILogger<AuditEventConsumer> logger)
    {
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.AuditLogs.AddAsync(auditLog, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Audit log {Id} saved successfully", auditLog.Id);
    }
}

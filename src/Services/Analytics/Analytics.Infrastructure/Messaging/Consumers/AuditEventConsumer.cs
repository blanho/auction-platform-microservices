using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Application.Abstractions;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Common.Contracts.Events;
using Analytics.Domain.Entities;
using DomainAuditAction = Analytics.Domain.Enums.AuditAction;
using Analytics.Application.Interfaces;
using Analytics.Infrastructure.Persistence;

namespace Analytics.Infrastructure.Messaging.Consumers;

public class AuditEventConsumer : IConsumer<AuditEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<AuditEventConsumer> _logger;

    public AuditEventConsumer(
        AnalyticsDbContext context,
        ILogger<AuditEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditEvent> context)
    {
        var auditEvent = context.Message;

        var exists = await _context.AuditLogs.AnyAsync(a => a.Id == auditEvent.Id, context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate audit event {EventId} skipped for {EntityType} ({EntityId})",
                auditEvent.Id, auditEvent.EntityType, auditEvent.EntityId);
            return;
        }

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
            Action = (DomainAuditAction)(int)auditEvent.Action,
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

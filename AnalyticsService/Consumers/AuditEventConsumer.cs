using System.Text.Json;
using Common.Audit.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using AnalyticsService.Domain.Entities;
using AnalyticsService.Interfaces;

namespace AnalyticsService.Consumers;

public class AuditEventConsumer : IConsumer<AuditEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuditEventConsumer> _logger;
    private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);
    private const string IdempotencyKeyPrefix = "audit:idempotency:";

    public AuditEventConsumer(
        IUnitOfWork unitOfWork,
        IDistributedCache cache,
        ILogger<AuditEventConsumer> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditEvent> context)
    {
        var auditEvent = context.Message;
        var idempotencyKey = $"{IdempotencyKeyPrefix}{auditEvent.Id}";
        var existingValue = await _cache.GetStringAsync(idempotencyKey, context.CancellationToken);
        if (!string.IsNullOrEmpty(existingValue))
        {
            _logger.LogInformation(
                "Audit event {AuditEventId} already processed, skipping",
                auditEvent.Id);
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
        await _cache.SetStringAsync(
            idempotencyKey,
            "processed",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = IdempotencyTtl },
            context.CancellationToken);

        _logger.LogInformation("Audit log {Id} saved successfully", auditLog.Id);
    }
}

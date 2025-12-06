using Common.Audit.Enums;

namespace UtilityService.DTOs;

public record AuditLogDto
{
    public Guid Id { get; init; }
    public Guid EntityId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public AuditAction Action { get; init; }
    public string ActionName => Action.ToString();
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public List<string>? ChangedProperties { get; init; }
    public Guid UserId { get; init; }
    public string? Username { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? CorrelationId { get; init; }
    public string? IpAddress { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public record AuditLogQueryParams
{
    public Guid? EntityId { get; init; }
    public string? EntityType { get; init; }
    public Guid? UserId { get; init; }
    public string? ServiceName { get; init; }
    public AuditAction? Action { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record PagedAuditLogsDto
{
    public List<AuditLogDto> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

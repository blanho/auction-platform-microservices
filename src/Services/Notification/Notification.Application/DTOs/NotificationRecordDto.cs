using BuildingBlocks.Application.Paging;

namespace Notification.Application.DTOs;

public class NotificationRecordDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TemplateKey { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Recipient { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? ExternalId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class NotificationRecordFilter
{
    public Guid? UserId { get; init; }
    public string? Channel { get; init; }
    public string? Status { get; init; }
    public string? TemplateKey { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
}

public class NotificationRecordFilterDto : QueryParameters<NotificationRecordFilter> { }

public class NotificationRecordStatsDto
{
    public int TotalRecords { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public Dictionary<string, int> ByChannel { get; set; } = new();
    public Dictionary<string, int> ByTemplate { get; set; } = new();
}

public record NotificationRecordFilterRequest(
    Guid? UserId,
    string? Channel,
    string? Status,
    string? TemplateKey,
    DateTimeOffset? FromDate,
    DateTimeOffset? ToDate,
    int? Page,
    int? PageSize);

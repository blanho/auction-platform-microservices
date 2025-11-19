using Analytics.Api.Enums;

namespace Analytics.Api.Entities;

public class Report
{
    public Guid Id { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public string ReportedUsername { get; set; } = string.Empty;
    public Guid? AuctionId { get; set; }
    public ReportType Type { get; set; }
    public ReportPriority Priority { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public string? Resolution { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public string? AdminNotes { get; set; }
    public DateTimeOffset? EscalatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

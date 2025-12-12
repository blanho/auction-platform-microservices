using UtilityService.Domain.Entities;

namespace UtilityService.DTOs;

public class ReportDto
{
    public Guid Id { get; set; }
    public string ReporterUsername { get; set; } = string.Empty;
    public string ReportedUsername { get; set; } = string.Empty;
    public Guid? AuctionId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Resolution { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class CreateReportDto
{
    public required string ReportedUsername { get; set; }
    public Guid? AuctionId { get; set; }
    public required ReportType Type { get; set; }
    public required string Reason { get; set; }
    public string? Description { get; set; }
}

public class ResolveReportDto
{
    public required string Resolution { get; set; }
    public required ReportStatus Status { get; set; }
}

public class ReportQueryParams
{
    public ReportStatus? Status { get; set; }
    public ReportType? Type { get; set; }
    public ReportPriority? Priority { get; set; }
    public string? ReportedUsername { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PagedReportsDto
{
    public List<ReportDto> Reports { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class ReportStatsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int UnderReviewReports { get; set; }
    public int ResolvedReports { get; set; }
    public int DismissedReports { get; set; }
    public int CriticalReports { get; set; }
    public int HighPriorityReports { get; set; }
}

public class UpdateReportStatusDto
{
    public ReportStatus Status { get; set; }
    public string? Resolution { get; set; }
}

using Analytics.Api.Entities;
using Analytics.Api.Models;

namespace Analytics.Api.Extensions.Mappings;

public static class ReportMappingExtensions
{
    public static ReportDto ToDto(this Report report)
    {
        return new ReportDto
        {
            Id = report.Id,
            ReporterUsername = report.ReporterUsername,
            ReportedUsername = report.ReportedUsername,
            AuctionId = report.AuctionId,
            Type = report.Type.ToString(),
            Priority = report.Priority.ToString(),
            Reason = report.Reason,
            Description = report.Description,
            Status = report.Status.ToString(),
            Resolution = report.Resolution,
            ResolvedBy = report.ResolvedBy,
            ResolvedAt = report.ResolvedAt,
            CreatedAt = report.CreatedAt
        };
    }

    public static List<ReportDto> ToDtoList(this IEnumerable<Report> reports)
    {
        return reports.Select(r => r.ToDto()).ToList();
    }
}

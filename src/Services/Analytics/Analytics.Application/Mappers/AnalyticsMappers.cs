using Analytics.Domain.Entities;
using Analytics.Application.DTOs;

namespace Analytics.Application.Mappers;

public static class AnalyticsMappers
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

    public static PlatformSettingDto ToDto(this PlatformSetting setting)
    {
        return new PlatformSettingDto
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value,
            Description = setting.Description,
            Category = setting.Category.ToString(),
            DataType = setting.DataType,
            IsSystem = setting.IsSystem,
            UpdatedAt = setting.UpdatedAt,
            UpdatedBy = setting.LastModifiedBy
        };
    }

    public static AuditLogDto ToDto(this AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            EntityId = log.EntityId,
            EntityType = log.EntityType,
            Action = log.Action,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            ChangedProperties = string.IsNullOrEmpty(log.ChangedProperties) ? null : System.Text.Json.JsonSerializer.Deserialize<List<string>>(log.ChangedProperties),
            UserId = log.UserId,
            Username = log.Username,
            ServiceName = log.ServiceName,
            CorrelationId = log.CorrelationId,
            IpAddress = log.IpAddress,
            Timestamp = log.Timestamp
        };
    }

    public static List<ReportDto> ToDtoList(this IEnumerable<Report> reports)
    {
        return reports.Select(r => r.ToDto()).ToList();
    }

    public static List<PlatformSettingDto> ToDtoList(this IEnumerable<PlatformSetting> settings)
    {
        return settings.Select(s => s.ToDto()).ToList();
    }

    public static List<AuditLogDto> ToDtoList(this IEnumerable<AuditLog> logs)
    {
        return logs.Select(l => l.ToDto()).ToList();
    }
}

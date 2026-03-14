using BuildingBlocks.Application.Abstractions;

namespace Analytics.Api.Errors;

public static class AnalyticsErrors
{
    public static class Report
    {
        public static Error NotFound => Error.Create("Report.NotFound", "Report not found");
        public static Error AlreadyResolved => Error.Create("Report.AlreadyResolved", "Report is already resolved");
        public static Error CannotDeleteResolved => Error.Create("Report.CannotDeleteResolved", "Cannot delete a resolved report");
        public static Error InvalidStatus => Error.Create("Report.InvalidStatus", "Invalid report status transition");
        public static Error CreateFailed => Error.Create("Report.CreateFailed", "Failed to create report");
        public static Error UpdateFailed => Error.Create("Report.UpdateFailed", "Failed to update report");
        public static Error DeleteFailed => Error.Create("Report.DeleteFailed", "Failed to delete report");
    }

    public static class Setting
    {
        public static Error NotFound => Error.Create("Setting.NotFound", "Setting not found");
        public static Error KeyExists => Error.Create("Setting.KeyExists", "Setting with this key already exists");
        public static Error SystemSettingReadOnly => Error.Create("Setting.SystemSettingReadOnly", "System settings cannot be modified");
        public static Error InvalidValue => Error.Create("Setting.InvalidValue", "Invalid setting value");
        public static Error CreateFailed => Error.Create("Setting.CreateFailed", "Failed to create setting");
        public static Error UpdateFailed => Error.Create("Setting.UpdateFailed", "Failed to update setting");
        public static Error DeleteFailed => Error.Create("Setting.DeleteFailed", "Failed to delete setting");
    }

    public static class AuditLog
    {
        public static Error NotFound => Error.Create("AuditLog.NotFound", "Audit log not found");
        public static Error QueryFailed => Error.Create("AuditLog.QueryFailed", "Failed to query audit logs");
    }

    public static class Analytics
    {
        public static Error QueryFailed => Error.Create("Analytics.QueryFailed", "Failed to retrieve analytics data");
        public static Error InvalidDateRange => Error.Create("Analytics.InvalidDateRange", "Invalid date range specified");
        public static Error InvalidPeriod => Error.Create("Analytics.InvalidPeriod", "Invalid period specified");
    }
}

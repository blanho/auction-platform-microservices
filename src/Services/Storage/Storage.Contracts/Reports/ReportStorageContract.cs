namespace StorageService.Contracts.Reports;

public static class ReportStorageContract
{
    public const string UploadPath = "/api/v1/internal/reports";
    public const string ApiKeyHeader = "X-Internal-Api-Key";
    public const string FileNameHeader = "X-Report-File-Name";
    public const string OwnerIdHeader = "X-Report-Owner-Id";
    public const string PrivateFolder = "private-reports";
}

public record StoredReportResponse(Guid FileId, string DownloadUrl);

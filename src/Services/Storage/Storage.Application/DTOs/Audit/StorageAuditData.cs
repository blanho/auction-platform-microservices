using Storage.Domain.Entities;

namespace Storage.Application.DTOs.Audit;

public sealed record StoredFileAuditData(
    Guid FileId,
    string FileName,
    string ContentType,
    long FileSize,
    string Status,
    string Provider,
    Guid? OwnerId)
{
    public static StoredFileAuditData FromStoredFile(StoredFile file) => new(
        file.Id,
        file.FileName,
        file.ContentType,
        file.FileSize,
        file.Status.ToString(),
        file.Provider.ToString(),
        file.OwnerId);
}

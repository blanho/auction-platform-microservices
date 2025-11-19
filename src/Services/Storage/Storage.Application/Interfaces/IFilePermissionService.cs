using Storage.Domain.Entities;

namespace Storage.Application.Interfaces;

public interface IFilePermissionService
{

    Task<PermissionCheckResult> CanDownloadAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default);

    Task<PermissionCheckResult> CanDeleteAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default);

    Task<PermissionCheckResult> CanModifyAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasResourceAccessAsync(
        Guid resourceId,
        Domain.Enums.StorageResourceType resourceType,
        string? userId,
        CancellationToken cancellationToken = default);
}

public class PermissionCheckResult
{
    public bool IsAllowed { get; private set; }
    public string? DenialReason { get; private set; }
    public bool NotFound { get; private set; }

    private PermissionCheckResult() { }

    public static PermissionCheckResult Allowed() => new() { IsAllowed = true };

    public static PermissionCheckResult Denied(string reason) => new()
    {
        IsAllowed = false,
        DenialReason = reason
    };

    public static PermissionCheckResult FileNotFound() => new()
    {
        IsAllowed = false,
        NotFound = true,
        DenialReason = "File not found"
    };
}

using Storage.Application.Interfaces;
using Storage.Domain.Enums;

namespace Storage.Application.Services;

public class FilePermissionService : IFilePermissionService
{
    private readonly IStoredFileRepository _repository;

    private static readonly HashSet<string> AdminRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "SystemAdmin", "SuperAdmin", "StorageAdmin"
    };

    public FilePermissionService(IStoredFileRepository repository)
    {
        _repository = repository;
    }

    public async Task<PermissionCheckResult> CanDownloadAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _repository.GetByIdAsync(fileId, cancellationToken);

        if (file == null)
        {
            return PermissionCheckResult.FileNotFound();
        }

        if (!file.IsAvailableForDownload)
        {
            return PermissionCheckResult.Denied($"File is not available for download. Status: {file.Status}");
        }

        var roles = userRoles?.ToList() ?? new List<string>();
        if (roles.Any(r => AdminRoles.Contains(r)))
        {
            return PermissionCheckResult.Allowed();
        }

        var userIdGuid = Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : (Guid?)null;
        if (userId != null && (file.UploadedBy == userId || file.OwnerId == userId))
        {
            return PermissionCheckResult.Allowed();
        }

        if (userIdGuid.HasValue && file.Permissions?.AllowedUsers?.Contains(userIdGuid.Value) == true)
        {
            return PermissionCheckResult.Allowed();
        }

        if (file.Permissions?.AllowAnonymous == true)
        {

            if (file.Permissions.MaxDownloads.HasValue &&
                file.Permissions.DownloadCount >= file.Permissions.MaxDownloads)
            {
                return PermissionCheckResult.Denied("Download limit exceeded");
            }
            return PermissionCheckResult.Allowed();
        }

        if (file.Permissions?.AllowedRoles != null && roles.Any())
        {
            if (file.Permissions.AllowedRoles.Any(r => roles.Contains(r)))
            {
                return PermissionCheckResult.Allowed();
            }
        }

        if (file.ResourceId.HasValue && file.ResourceType.HasValue && userId != null)
        {
            var hasResourceAccess = await HasResourceAccessAsync(
                file.ResourceId.Value,
                file.ResourceType.Value,
                userId,
                cancellationToken);

            if (hasResourceAccess)
            {
                return PermissionCheckResult.Allowed();
            }
        }

        return PermissionCheckResult.Denied("Access not authorized");
    }

    public async Task<PermissionCheckResult> CanDeleteAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default)
    {
        var file = await _repository.GetByIdAsync(fileId, cancellationToken);

        if (file == null)
        {
            return PermissionCheckResult.FileNotFound();
        }

        var roles = userRoles?.ToList() ?? new List<string>();
        if (roles.Any(r => AdminRoles.Contains(r)))
        {
            return PermissionCheckResult.Allowed();
        }

        if (userId != null && (file.UploadedBy == userId || file.OwnerId == userId))
        {
            return PermissionCheckResult.Allowed();
        }

        return PermissionCheckResult.Denied("Only file owner or admin can delete");
    }

    public async Task<PermissionCheckResult> CanModifyAsync(
        Guid fileId,
        string? userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default)
    {

        return await CanDeleteAsync(fileId, userId, userRoles, cancellationToken);
    }

    public Task<bool> HasResourceAccessAsync(
        Guid resourceId,
        StorageResourceType resourceType,
        string? userId,
        CancellationToken cancellationToken = default)
    {

        return Task.FromResult(true);
    }
}

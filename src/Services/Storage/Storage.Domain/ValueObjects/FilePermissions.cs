#nullable enable
using Storage.Domain.Enums;

namespace Storage.Domain.ValueObjects;

public class FilePermissions
{

    public FileAccessLevel ReadAccess { get; set; } = FileAccessLevel.Owner;

    public List<Guid>? AllowedUsers { get; set; }

    public List<string>? AllowedRoles { get; set; }

    public bool AllowAnonymous { get; set; }

    public int? MaxDownloads { get; set; }

    public int DownloadCount { get; set; }

    public List<string>? AllowedIpRanges { get; set; }

    public string? RequiredReferer { get; set; }

    public bool HasAccess(Guid userId, IEnumerable<string>? userRoles = null)
    {
        if (AllowAnonymous)
            return true;

        if (AllowedUsers?.Contains(userId) == true)
            return true;

        if (userRoles != null && AllowedRoles != null)
        {
            if (AllowedRoles.Any(r => userRoles.Contains(r)))
                return true;
        }

        return false;
    }

    public bool CanDownload()
    {
        if (!MaxDownloads.HasValue)
            return true;

        return DownloadCount < MaxDownloads.Value;
    }

    public void IncrementDownloadCount()
    {
        DownloadCount++;
    }

    public static FilePermissions OwnerOnly() => new()
    {
        ReadAccess = FileAccessLevel.Owner,
        AllowAnonymous = false
    };

    public static FilePermissions Public() => new()
    {
        ReadAccess = FileAccessLevel.Public,
        AllowAnonymous = true
    };

    public static FilePermissions TenantWide() => new()
    {
        ReadAccess = FileAccessLevel.Tenant,
        AllowAnonymous = false
    };
}

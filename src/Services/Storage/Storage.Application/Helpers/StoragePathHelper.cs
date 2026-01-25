using Storage.Application.Configuration;
using Storage.Domain.Enums;

namespace Storage.Application.Helpers;

public static class StoragePathHelper
{
    public static StorageProvider GetStorageProvider(string defaultProvider)
    {
        return defaultProvider.ToLowerInvariant() switch
        {
            "azure" or "blob" => StorageProvider.AzureBlob,
            _ => StorageProvider.Local
        };
    }

    public static (string containerName, string blobPath) ParseStoragePath(string path, StorageOptions options)
    {
        var parts = path.Split('/', 2);
        if (parts.Length == 2 &&
            !path.StartsWith(options.TempFolder) &&
            !path.StartsWith(options.PermanentFolder))
        {
            return (parts[0], parts[1]);
        }

        var container = path.StartsWith(options.TempFolder)
            ? options.Azure.TempContainerName
            : options.Azure.MediaContainerName;

        return (container, path);
    }
}

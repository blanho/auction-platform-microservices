using Storage.Application.Configuration;

namespace Storage.Application.Helpers;

public static class FileValidationHelper
{
    public static string? ValidateFile(string fileName, string contentType, long size, StorageOptions options)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !options.AllowedExtensions.Contains(extension))
        {
            return $"File extension '{extension}' is not allowed";
        }

        if (!options.AllowedContentTypes.Contains(contentType))
        {
            return $"Content type '{contentType}' is not allowed";
        }

        if (size > options.MaxFileSize)
        {
            return $"File size exceeds maximum allowed size of {options.MaxFileSize / 1024 / 1024}MB";
        }

        return null;
    }

    public static bool IsExtensionAllowed(string fileName, IEnumerable<string> allowedExtensions)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return !string.IsNullOrEmpty(extension) && allowedExtensions.Contains(extension);
    }

    public static bool IsContentTypeAllowed(string contentType, IEnumerable<string> allowedContentTypes)
    {
        return allowedContentTypes.Contains(contentType);
    }

    public static bool IsSizeValid(long size, long maxSize)
    {
        return size <= maxSize;
    }
}

using BuildingBlocks.Application.Abstractions;

namespace Storage.Application.Errors;

public static class StorageErrors
{
    public static class File
    {
        public static Error NotFound => Error.Create("File.NotFound", "File not found");
        public static Error NotFoundById(Guid id) => Error.Create("File.NotFound", $"File with ID {id} was not found");
        public static Error NotFoundByPath(string path) => Error.Create("File.NotFound", $"File '{path}' was not found");
        public static Error UploadFailed(string reason) => Error.Create("File.UploadFailed", $"Failed to upload file: {reason}");
        public static Error DownloadFailed(string reason) => Error.Create("File.DownloadFailed", $"Failed to download file: {reason}");
        public static Error DeleteFailed(string reason) => Error.Create("File.DeleteFailed", $"Failed to delete file: {reason}");
        public static Error InvalidType => Error.Create("File.InvalidType", "Invalid file type");
        public static Error InvalidTypeWithDetails(string type) => Error.Create("File.InvalidType", $"File type '{type}' is not allowed");
        public static Error TooLarge => Error.Create("File.TooLarge", "File exceeds maximum allowed size");
        public static Error TooLargeWithDetails(long maxSize) => Error.Create("File.TooLarge", $"File exceeds maximum allowed size of {maxSize / 1024 / 1024}MB");
        public static Error Empty => Error.Create("File.Empty", "File is empty");
        public static Error AlreadyExists => Error.Create("File.AlreadyExists", "File already exists");
    }

    public static class Permission
    {
        public static Error Denied => Error.Create("Permission.Denied", "Access denied");
        public static Error NotFound => Error.Create("Permission.NotFound", "Permission not found");
        public static Error InvalidOperation => Error.Create("Permission.InvalidOperation", "Invalid permission operation");
    }

    public static class Folder
    {
        public static Error NotFound => Error.Create("Folder.NotFound", "Folder not found");
        public static Error NotFoundByPath(string path) => Error.Create("Folder.NotFound", $"Folder '{path}' was not found");
        public static Error CreateFailed(string reason) => Error.Create("Folder.CreateFailed", $"Failed to create folder: {reason}");
        public static Error DeleteFailed(string reason) => Error.Create("Folder.DeleteFailed", $"Failed to delete folder: {reason}");
        public static Error NotEmpty => Error.Create("Folder.NotEmpty", "Folder is not empty");
    }

    public static class Storage
    {
        public static Error QuotaExceeded => Error.Create("Storage.QuotaExceeded", "Storage quota exceeded");
        public static Error Unavailable => Error.Create("Storage.Unavailable", "Storage service is unavailable");
        public static Error ConfigurationError(string reason) => Error.Create("Storage.ConfigurationError", $"Storage configuration error: {reason}");
    }
}

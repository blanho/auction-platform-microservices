namespace Storage.Application.Errors;

public static class StorageErrors
{
    public static Error FileNotFound(Guid fileId) =>
        Error.Create("Storage.FileNotFound", $"File with ID '{fileId}' was not found");

    public static Error FileNotFound(string fileId) =>
        Error.Create("Storage.FileNotFound", $"File with ID '{fileId}' was not found");

    public static readonly Error EmptyFile =
        Error.Create("Storage.EmptyFile", "File is empty");

    public static Error FileTooLarge(long maxSizeMb) =>
        Error.Create("Storage.FileTooLarge", $"File size exceeds maximum allowed size of {maxSizeMb}MB");

    public static Error InvalidExtension(string extension, string allowed) =>
        Error.Create("Storage.InvalidExtension", $"File extension '{extension}' is not allowed. Allowed: {allowed}");

    public static Error InvalidContentType(string contentType) =>
        Error.Create("Storage.InvalidContentType", $"Content type '{contentType}' is not allowed");

    public static readonly Error NoFilesProvided =
        Error.Create("Storage.NoFiles", "No files provided");

    public static Error TooManyFiles(int max) =>
        Error.Create("Storage.TooManyFiles", $"Maximum {max} files allowed per upload");

    public static readonly Error DeleteFailed =
        Error.Create("Storage.DeleteFailed", "Failed to delete file from storage");
}

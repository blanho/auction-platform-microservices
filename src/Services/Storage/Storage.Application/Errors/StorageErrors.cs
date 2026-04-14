using BuildingBlocks.Application.Abstractions;

namespace Storage.Application.Errors;

public static class StorageErrors
{
    public static Error FileNotFound(Guid fileId) =>
        LocalizableError.Localizable("Storage.FileNotFound", $"File with ID '{fileId}' was not found", fileId);

    public static Error FileNotFound(string fileId) =>
        LocalizableError.Localizable("Storage.FileNotFound", $"File with ID '{fileId}' was not found", fileId);

    public static readonly Error EmptyFile =
        Error.Create("Storage.EmptyFile", "File is empty");

    public static Error FileTooLarge(long maxSizeMb) =>
        LocalizableError.Localizable("Storage.FileTooLarge", $"File size exceeds maximum allowed size of {maxSizeMb}MB", maxSizeMb);

    public static Error InvalidExtension(string extension, string allowed) =>
        LocalizableError.Localizable("Storage.InvalidExtension", $"File extension '{extension}' is not allowed. Allowed: {allowed}", extension, allowed);

    public static Error InvalidContentType(string contentType) =>
        LocalizableError.Localizable("Storage.InvalidContentType", $"Content type '{contentType}' is not allowed", contentType);

    public static readonly Error NoFilesProvided =
        Error.Create("Storage.NoFiles", "No files provided");

    public static Error TooManyFiles(int max) =>
        LocalizableError.Localizable("Storage.TooManyFiles", $"Maximum {max} files allowed per upload", max);

    public static readonly Error DeleteFailed =
        Error.Create("Storage.DeleteFailed", "Failed to delete file from storage");

    public static readonly Error PresignedUrlNotSupported =
        Error.Create("Storage.PresignedUrlNotSupported", "Presigned URL generation is not supported by the current storage provider");

    public static readonly Error FileNotFoundInStorage =
        Error.Create("Storage.FileNotFoundInStorage", "File was not found in the storage backend. Ensure the file was uploaded before confirming.");

    public static Error UrlGenerationFailed(Guid fileId) =>
        LocalizableError.Localizable("Storage.UrlGenerationFailed", $"Failed to generate a URL for file '{fileId}'", fileId);

    public static readonly Error BatchUploadFailed =
        Error.Create("Storage.BatchUploadFailed", "All file uploads failed");
}

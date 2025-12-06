using Common.Utilities.Constants;

namespace Common.Utilities.Helpers;

public static class FileHelper
{
    public static bool IsValidExcelExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return FileConstants.AllowedExtensions.Excel.Contains(extension);
    }
    public static bool IsValidImageExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return FileConstants.AllowedExtensions.Images.Contains(extension);
    }

    public static bool IsValidDocumentExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return FileConstants.AllowedExtensions.Documents.Contains(extension);
    }

    public static bool IsValidExtension(string fileName, params string[] allowedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }

    public static string GenerateExportFileName(string baseName, string extension)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"{baseName}_{timestamp}{extension}";
    }

    public static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => FileConstants.ContentTypes.Excel,
            ".xls" => FileConstants.ContentTypes.ExcelLegacy,
            ".csv" => FileConstants.ContentTypes.Csv,
            ".json" => FileConstants.ContentTypes.Json,
            ".pdf" => FileConstants.ContentTypes.Pdf,
            ".xml" => FileConstants.ContentTypes.Xml,
            ".zip" => FileConstants.ContentTypes.Zip,
            _ => FileConstants.ContentTypes.OctetStream
        };
    }

    public static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? "file" : sanitized;
    }
}

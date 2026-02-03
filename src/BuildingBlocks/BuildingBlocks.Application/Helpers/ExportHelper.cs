using System.Text;
using System.Text.Json;

namespace BuildingBlocks.Application.Helpers;

public static class ExportHelper
{
    public static byte[] GenerateCsv<T>(IEnumerable<T> items, string[] headers, Func<T, string?[]> valueSelector)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers));

        foreach (var item in items)
        {
            var values = valueSelector(item);
            var escapedValues = values.Select(v => $"\"{EscapeCsvValue(v)}\"");
            sb.AppendLine(string.Join(",", escapedValues));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public static byte[] GenerateJson<T>(IEnumerable<T> items, bool writeIndented = true)
    {
        return JsonSerializer.SerializeToUtf8Bytes(items, new JsonSerializerOptions
        {
            WriteIndented = writeIndented
        });
    }

    public static string EscapeCsvValue(string? value)
    {
        return value?.Replace("\"", "\"\"") ?? "";
    }

    public static string GenerateExportFileName(string prefix, string extension)
    {
        return $"{prefix}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{extension}";
    }

    public static string GetFileExtension(string format)
    {
        return format.ToLowerInvariant() switch
        {
            "csv" => "csv",
            "json" => "json",
            "pdf" => "pdf",
            _ => "xlsx"
        };
    }

    public static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}

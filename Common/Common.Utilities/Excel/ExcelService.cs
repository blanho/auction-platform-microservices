using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace Common.Utilities.Excel;

public class ExcelService : IExcelService
{
    private readonly ILogger<ExcelService>? _logger;

    public ExcelService(ILogger<ExcelService>? logger = null)
    {
        _logger = logger;
    }

    public List<T> ParseFile<T>(Stream fileStream, IEnumerable<ExcelColumnConfig>? columnConfigs = null) where T : new()
    {
        var results = new List<T>();
        var configs = columnConfigs?.ToList();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);
        var columnMap = MapColumns(worksheet.Row(1));
        var lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var row = 2; row <= lastRowUsed; row++)
        {
            try
            {
                var worksheetRow = worksheet.Row(row);
                if (worksheetRow.IsEmpty())
                    continue;

                var item = new T();
                var type = typeof(T);

                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        var property = type.GetProperty(config.PropertyName);
                        if (property == null) continue;

                        var value = GetCellValue(worksheetRow, columnMap, config.Header);
                        
                        if (string.IsNullOrEmpty(value) && config.DefaultValue != null)
                        {
                            property.SetValue(item, config.DefaultValue);
                        }
                        else if (config.Parser != null)
                        {
                            property.SetValue(item, config.Parser(value));
                        }
                        else
                        {
                            var convertedValue = ConvertValue(value, property.PropertyType);
                            property.SetValue(item, convertedValue);
                        }
                    }
                }
                else
                {
                    foreach (var property in type.GetProperties())
                    {
                        if (!columnMap.ContainsKey(property.Name)) continue;
                        
                        var value = GetCellValue(worksheetRow, columnMap, property.Name);
                        var convertedValue = ConvertValue(value, property.PropertyType);
                        property.SetValue(item, convertedValue);
                    }
                }

                results.Add(item);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse row {RowNumber}", row);
            }
        }

        _logger?.LogInformation("Parsed {Count} items from Excel file", results.Count);
        return results;
    }

    public List<T> ParseFile<T>(Stream fileStream, Func<IExcelRowReader, T?> rowParser)
    {
        var results = new List<T>();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);
        var columnMap = MapColumns(worksheet.Row(1));
        var lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var row = 2; row <= lastRowUsed; row++)
        {
            try
            {
                var worksheetRow = worksheet.Row(row);
                if (worksheetRow.IsEmpty())
                    continue;

                var reader = new ExcelRowReader(worksheetRow, columnMap, row);
                var item = rowParser(reader);
                
                if (item != null)
                {
                    results.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to parse row {RowNumber}", row);
            }
        }

        _logger?.LogInformation("Parsed {Count} items from Excel file", results.Count);
        return results;
    }

    public byte[] GenerateFile<T>(
        IEnumerable<T> data,
        string[] headers,
        Func<T, object?[]> valueSelector,
        ExcelExportConfig? config = null)
    {
        config ??= new ExcelExportConfig();
        
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(config.SheetName);

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            
            if (!string.IsNullOrEmpty(config.HeaderBackgroundColor))
            {
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml($"#{config.HeaderBackgroundColor}");
            }
            
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        var dataList = data.ToList();
        for (var row = 0; row < dataList.Count; row++)
        {
            var values = valueSelector(dataList[row]);
            var excelRow = row + 2;

            for (var col = 0; col < values.Length && col < headers.Length; col++)
            {
                var cell = worksheet.Cell(excelRow, col + 1);
                SetCellValue(cell, values[col]);
            }
        }

        if (config.AutoFitColumns)
        {
            worksheet.Columns().AdjustToContents();
        }

        if (config.AddAutoFilter)
        {
            worksheet.RangeUsed()?.SetAutoFilter();
        }

        if (config.FreezeHeaderRow)
        {
            worksheet.SheetView.FreezeRows(1);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        
        _logger?.LogInformation("Generated Excel export with {Count} rows", dataList.Count);
        return stream.ToArray();
    }

    public byte[] GenerateTemplate(string[] headers, ExcelTemplateConfig? config = null)
    {
        config ??= new ExcelTemplateConfig();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(config.SheetName);

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;

            if (!string.IsNullOrEmpty(config.HeaderBackgroundColor))
            {
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml($"#{config.HeaderBackgroundColor}");
            }

            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        if (config.SampleData != null)
        {
            for (var row = 0; row < config.SampleData.Count; row++)
            {
                var rowData = config.SampleData[row];
                var excelRow = row + 2;

                for (var col = 0; col < headers.Length; col++)
                {
                    if (rowData.TryGetValue(headers[col], out var value))
                    {
                        SetCellValue(worksheet.Cell(excelRow, col + 1), value);
                    }
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        if (config.IncludeInstructions && config.Instructions?.Count > 0)
        {
            var instructionsSheet = workbook.Worksheets.Add(config.InstructionsSheetName);
            
            instructionsSheet.Cell(1, 1).Value = "Import Instructions";
            instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(1, 1).Style.Font.FontSize = 14;

            for (var i = 0; i < config.Instructions.Count; i++)
            {
                instructionsSheet.Cell(i + 3, 1).Value = config.Instructions[i];
            }

            instructionsSheet.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        _logger?.LogInformation("Generated Excel import template");
        return stream.ToArray();
    }

    private static Dictionary<string, int> MapColumns(IXLRow headerRow)
    {
        var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var headerName = cell.GetString().Trim();
            if (!string.IsNullOrEmpty(headerName))
            {
                columnMap[headerName] = cell.Address.ColumnNumber;
            }
        }

        return columnMap;
    }

    private static string GetCellValue(IXLRow row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out var columnNumber))
            return string.Empty;

        return row.Cell(columnNumber).GetString().Trim();
    }

    private static object? ConvertValue(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return underlyingType switch
        {
            _ when underlyingType == typeof(string) => value,
            _ when underlyingType == typeof(int) => int.TryParse(value, out var i) ? i : 0,
            _ when underlyingType == typeof(long) => long.TryParse(value, out var l) ? l : 0L,
            _ when underlyingType == typeof(decimal) => decimal.TryParse(value, out var d) ? d : 0m,
            _ when underlyingType == typeof(double) => double.TryParse(value, out var db) ? db : 0d,
            _ when underlyingType == typeof(float) => float.TryParse(value, out var f) ? f : 0f,
            _ when underlyingType == typeof(bool) => bool.TryParse(value, out var b) && b,
            _ when underlyingType == typeof(DateTime) => DateTime.TryParse(value, out var dt) ? dt : DateTime.MinValue,
            _ when underlyingType == typeof(DateTimeOffset) => DateTimeOffset.TryParse(value, out var dto) ? dto : DateTimeOffset.MinValue,
            _ when underlyingType == typeof(Guid) => Guid.TryParse(value, out var g) ? g : Guid.Empty,
            _ when underlyingType.IsEnum => Enum.TryParse(underlyingType, value, true, out var e) ? e : Activator.CreateInstance(underlyingType),
            _ => value
        };
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = "";
                break;
            case DateTime dt:
                cell.Value = dt.ToString("yyyy-MM-dd HH:mm:ss");
                break;
            case DateTimeOffset dto:
                cell.Value = dto.ToString("yyyy-MM-dd HH:mm:ss");
                break;
            case bool b:
                cell.Value = b ? "Yes" : "No";
                break;
            case decimal d:
                cell.Value = d;
                break;
            case double db:
                cell.Value = db;
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }
}

/// <summary>
/// Implementation of IExcelRowReader
/// </summary>
internal class ExcelRowReader : IExcelRowReader
{
    private readonly IXLRow _row;
    private readonly Dictionary<string, int> _columnMap;

    public ExcelRowReader(IXLRow row, Dictionary<string, int> columnMap, int rowNumber)
    {
        _row = row;
        _columnMap = columnMap;
        RowNumber = rowNumber;
    }

    public bool IsEmpty => _row.IsEmpty();
    public int RowNumber { get; }

    public string GetString(string columnName)
    {
        if (!_columnMap.TryGetValue(columnName, out var columnNumber))
            return string.Empty;
        return _row.Cell(columnNumber).GetString().Trim();
    }

    public string GetString(int columnIndex)
    {
        return _row.Cell(columnIndex).GetString().Trim();
    }

    public int GetInt(string columnName, int defaultValue = 0)
    {
        var value = GetString(columnName);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public long GetLong(string columnName, long defaultValue = 0)
    {
        var value = GetString(columnName);
        return long.TryParse(value, out var result) ? result : defaultValue;
    }

    public decimal GetDecimal(string columnName, decimal defaultValue = 0)
    {
        var value = GetString(columnName);
        return decimal.TryParse(value, out var result) ? result : defaultValue;
    }

    public double GetDouble(string columnName, double defaultValue = 0)
    {
        var value = GetString(columnName);
        return double.TryParse(value, out var result) ? result : defaultValue;
    }

    public bool GetBool(string columnName, bool defaultValue = false)
    {
        var value = GetString(columnName).ToLowerInvariant();
        return value switch
        {
            "true" or "yes" or "1" => true,
            "false" or "no" or "0" => false,
            _ => defaultValue
        };
    }

    public DateTime? GetDateTime(string columnName)
    {
        if (!_columnMap.TryGetValue(columnName, out var columnNumber))
            return null;

        var cell = _row.Cell(columnNumber);

        if (cell.TryGetValue<DateTime>(out var dateTime))
            return dateTime;

        var stringValue = cell.GetString().Trim();
        return DateTime.TryParse(stringValue, out var result) ? result : null;
    }

    public DateTimeOffset? GetDateTimeOffset(string columnName)
    {
        var dateTime = GetDateTime(columnName);
        return dateTime.HasValue ? new DateTimeOffset(dateTime.Value, TimeSpan.Zero) : null;
    }

    public Guid? GetGuid(string columnName)
    {
        var value = GetString(columnName);
        return Guid.TryParse(value, out var result) ? result : null;
    }
}

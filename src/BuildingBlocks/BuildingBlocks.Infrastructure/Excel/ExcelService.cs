using BuildingBlocks.Application.Abstractions;
using ClosedXML.Excel;

namespace BuildingBlocks.Infrastructure.Excel;

public sealed class ExcelService : IExcelService
{
    public List<T> ParseFile<T>(Stream fileStream, IEnumerable<ExcelColumnConfig>? columnConfigs = null) where T : new()
    {
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);
        var range = worksheet.RangeUsed();

        if (range == null)
            return new List<T>();

        var rows = range.RowsUsed().Skip(1);

        var results = new List<T>();
        var properties = typeof(T).GetProperties();

        foreach (var row in rows)
        {
            var item = new T();
            foreach (var prop in properties)
            {
                var cell = row.Cell(GetColumnIndex(worksheet, prop.Name));
                var cellValue = cell.Value;

                if (!cellValue.IsBlank)
                {
                    var convertedValue = Convert.ChangeType(cellValue.ToString(), prop.PropertyType);
                    prop.SetValue(item, convertedValue);
                }
            }
            results.Add(item);
        }

        return results;
    }

    public List<T> ParseFile<T>(Stream fileStream, Func<IExcelRowReader, T?> rowParser)
    {
        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);

        var headerRow = worksheet.Row(1);
        var headerMap = new Dictionary<string, int>();

        for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
        {
            var headerName = headerRow.Cell(col).GetString();
            if (!string.IsNullOrWhiteSpace(headerName))
            {
                headerMap[headerName] = col;
            }
        }

        var results = new List<T>();
        var range = worksheet.RangeUsed();

        if (range == null)
            return results;

        var rows = range.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            var xlRow = worksheet.Row(row.RangeAddress.FirstAddress.RowNumber);
            var reader = new ExcelRowReader(xlRow, headerMap);
            var item = rowParser(reader);

            if (item != null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public byte[] GenerateFile<T>(IEnumerable<T> data, string[] headers, Func<T, object?[]> valueSelector, ExcelExportConfig? config = null)
    {
        config ??= new ExcelExportConfig();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(config.SheetName);

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;

            if (!string.IsNullOrEmpty(config.HeaderBackgroundColor))
            {
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#" + config.HeaderBackgroundColor);
            }
        }

        int rowIndex = 2;
        foreach (var item in data)
        {
            var values = valueSelector(item);
            for (int colIndex = 0; colIndex < values.Length; colIndex++)
            {
                worksheet.Cell(rowIndex, colIndex + 1).Value = values[colIndex]?.ToString() ?? string.Empty;
            }
            rowIndex++;
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
        return stream.ToArray();
    }

    public byte[] GenerateTemplate(string[] headers, ExcelTemplateConfig? config = null)
    {
        config ??= new ExcelTemplateConfig();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(config.SheetName);

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;

            if (!string.IsNullOrEmpty(config.HeaderBackgroundColor))
            {
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#" + config.HeaderBackgroundColor);
            }
        }

        if (config.SampleData != null && config.SampleData.Any())
        {
            int rowIndex = 2;
            foreach (var sampleRow in config.SampleData)
            {
                for (int colIndex = 0; colIndex < headers.Length; colIndex++)
                {
                    if (sampleRow.TryGetValue(headers[colIndex], out var value))
                    {
                        worksheet.Cell(rowIndex, colIndex + 1).Value = value?.ToString() ?? string.Empty;
                    }
                }
                rowIndex++;
            }
        }

        if (config.IncludeInstructions && config.Instructions != null && config.Instructions.Any())
        {
            var instructionsSheet = workbook.Worksheets.Add(config.InstructionsSheetName);
            instructionsSheet.Cell(1, 1).Value = "Instructions";
            instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(1, 1).Style.Font.FontSize = 14;

            for (int i = 0; i < config.Instructions.Count; i++)
            {
                instructionsSheet.Cell(i + 3, 1).Value = $"{i + 1}. {config.Instructions[i]}";
            }

            instructionsSheet.Columns().AdjustToContents();
        }

        worksheet.Columns().AdjustToContents();
        worksheet.RangeUsed()?.SetAutoFilter();
        worksheet.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static int GetColumnIndex(IXLWorksheet worksheet, string columnName)
    {
        var headerRow = worksheet.Row(1);
        for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
        {
            if (headerRow.Cell(col).GetString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                return col;
            }
        }
        throw new InvalidOperationException($"Column '{columnName}' not found in Excel file.");
    }

    private sealed class ExcelRowReader : IExcelRowReader
    {
        private readonly IXLRow _row;
        private readonly Dictionary<string, int> _headerMap;

        public ExcelRowReader(IXLRow row, Dictionary<string, int> headerMap)
        {
            _row = row;
            _headerMap = headerMap;
        }

        public bool IsEmpty => _row.Cells().All(c => c.IsEmpty());
        public int RowNumber => _row.RowNumber();

        public string GetString(string columnName)
        {
            if (!_headerMap.TryGetValue(columnName, out var colIndex))
                return string.Empty;

            return _row.Cell(colIndex).GetString();
        }

        public string GetString(int columnIndex)
        {
            return _row.Cell(columnIndex).GetString();
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
            var value = GetString(columnName);
            return bool.TryParse(value, out var result) ? result : defaultValue;
        }

        public DateTime? GetDateTime(string columnName)
        {
            if (!_headerMap.TryGetValue(columnName, out var colIndex))
                return null;

            var cell = _row.Cell(colIndex);
            if (cell.TryGetValue(out DateTime dateValue))
                return dateValue;

            return null;
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
}

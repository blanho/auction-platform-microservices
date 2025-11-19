namespace BuildingBlocks.Application.Abstractions;

public interface IExcelService
{
    List<T> ParseFile<T>(Stream fileStream, IEnumerable<ExcelColumnConfig>? columnConfigs = null) where T : new();
    List<T> ParseFile<T>(Stream fileStream, Func<IExcelRowReader, T?> rowParser);
    byte[] GenerateFile<T>(IEnumerable<T> data, string[] headers, Func<T, object?[]> valueSelector, ExcelExportConfig? config = null);
    byte[] GenerateTemplate(string[] headers, ExcelTemplateConfig? config = null);
}

public interface IExcelRowReader
{
    string GetString(string columnName);
    string GetString(int columnIndex);
    int GetInt(string columnName, int defaultValue = 0);
    long GetLong(string columnName, long defaultValue = 0);
    decimal GetDecimal(string columnName, decimal defaultValue = 0);
    double GetDouble(string columnName, double defaultValue = 0);
    bool GetBool(string columnName, bool defaultValue = false);
    DateTime? GetDateTime(string columnName);
    DateTimeOffset? GetDateTimeOffset(string columnName);
    Guid? GetGuid(string columnName);
    bool IsEmpty { get; }
    int RowNumber { get; }
}

public class ExcelColumnConfig
{
    public required string Header { get; init; }
    public required string PropertyName { get; init; }
    public bool IsRequired { get; init; }
    public object? DefaultValue { get; init; }
    public Func<string, object?>? Parser { get; init; }
}

public class ExcelExportConfig
{
    public string SheetName { get; init; } = "Data";
    public bool AutoFitColumns { get; init; } = true;
    public bool AddAutoFilter { get; init; } = true;
    public bool FreezeHeaderRow { get; init; } = true;
    public string? HeaderBackgroundColor { get; init; } = "ADD8E6";
}

public class ExcelTemplateConfig
{
    public string SheetName { get; init; } = "Import Template";
    public bool IncludeInstructions { get; init; } = true;
    public string InstructionsSheetName { get; init; } = "Instructions";
    public string? HeaderBackgroundColor { get; init; } = "90EE90";
    public List<Dictionary<string, object>>? SampleData { get; init; }
    public List<string>? Instructions { get; init; }
}

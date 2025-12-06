namespace Common.Utilities.Excel;

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

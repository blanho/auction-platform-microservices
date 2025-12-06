namespace Common.Utilities.Excel;

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

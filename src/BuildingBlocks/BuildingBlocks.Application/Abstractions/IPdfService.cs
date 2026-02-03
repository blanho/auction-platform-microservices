namespace BuildingBlocks.Application.Abstractions;

public interface IPdfService
{
    byte[] GenerateReport(PdfReportConfig config);
    byte[] GenerateTableReport<T>(IEnumerable<T> data, PdfTableReportConfig<T> config);
}

public class PdfReportConfig
{
    public string Title { get; set; } = "Report";
    public string? Subtitle { get; set; }
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<PdfReportSection> Sections { get; set; } = new();
    public PdfPageSettings PageSettings { get; set; } = new();
}

public class PdfPageSettings
{
    public bool IsLandscape { get; set; }
    public float MarginLeft { get; set; } = 40;
    public float MarginRight { get; set; } = 40;
    public float MarginTop { get; set; } = 40;
    public float MarginBottom { get; set; } = 40;
}

public class PdfReportSection
{
    public string Title { get; set; } = "";
    public PdfSectionType Type { get; set; } = PdfSectionType.Text;
    public string? TextContent { get; set; }
    public List<PdfKeyValuePair>? KeyValues { get; set; }
    public PdfTableData? TableData { get; set; }
}

public enum PdfSectionType
{
    Text,
    KeyValue,
    Table
}

public class PdfKeyValuePair
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public class PdfTableData
{
    public string[] Headers { get; set; } = Array.Empty<string>();
    public List<string[]> Rows { get; set; } = new();
}

public class PdfTableReportConfig<T>
{
    public string Title { get; set; } = "Report";
    public string? Subtitle { get; set; }
    public string[] Headers { get; set; } = Array.Empty<string>();
    public Func<T, string[]> RowSelector { get; set; } = _ => Array.Empty<string>();
    public PdfPageSettings PageSettings { get; set; } = new() { IsLandscape = true };
    public Dictionary<string, string>? SummaryData { get; set; }
}

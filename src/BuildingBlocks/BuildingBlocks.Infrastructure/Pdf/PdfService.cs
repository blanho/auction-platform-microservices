using BuildingBlocks.Application.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BuildingBlocks.Infrastructure.Pdf;

public sealed class PdfService : IPdfService
{
    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateReport(PdfReportConfig config)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, config.PageSettings);

                page.Header().Element(c => ComposeHeader(c, config.Title, config.Subtitle, config.GeneratedAt));

                page.Content().Element(c => ComposeContent(c, config.Sections));

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GenerateTableReport<T>(IEnumerable<T> data, PdfTableReportConfig<T> config)
    {
        var items = data.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                ConfigurePage(page, config.PageSettings);

                page.Header().Element(c => ComposeHeader(c, config.Title, config.Subtitle, DateTimeOffset.UtcNow));

                page.Content().Column(column =>
                {
                    if (config.SummaryData != null && config.SummaryData.Any())
                    {
                        column.Item().PaddingBottom(20).Element(c => ComposeSummary(c, config.SummaryData));
                    }

                    column.Item().Element(c => ComposeTable(c, config.Headers, items, config.RowSelector));
                });

                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ConfigurePage(PageDescriptor page, PdfPageSettings settings)
    {
        if (settings.IsLandscape)
            page.Size(PageSizes.A4.Landscape());
        else
            page.Size(PageSizes.A4);

        page.MarginLeft(settings.MarginLeft);
        page.MarginRight(settings.MarginRight);
        page.MarginTop(settings.MarginTop);
        page.MarginBottom(settings.MarginBottom);
        page.DefaultTextStyle(x => x.FontSize(10));
    }

    private static void ComposeHeader(IContainer container, string title, string? subtitle, DateTimeOffset generatedAt)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(innerColumn =>
                {
                    innerColumn.Item()
                        .Text(title)
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    if (!string.IsNullOrEmpty(subtitle))
                    {
                        innerColumn.Item()
                            .Text(subtitle)
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                row.ConstantItem(150).Column(innerColumn =>
                {
                    innerColumn.Item()
                        .AlignRight()
                        .Text($"Generated: {generatedAt:yyyy-MM-dd HH:mm}")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingBottom(20);
        });
    }

    private static void ComposeContent(IContainer container, List<PdfReportSection> sections)
    {
        container.Column(column =>
        {
            foreach (var section in sections)
            {
                column.Item().Element(c => ComposeSection(c, section));
                column.Item().PaddingBottom(15);
            }
        });
    }

    private static void ComposeSection(IContainer container, PdfReportSection section)
    {
        container.Column(column =>
        {
            if (!string.IsNullOrEmpty(section.Title))
            {
                column.Item()
                    .PaddingBottom(8)
                    .Text(section.Title)
                    .FontSize(14)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);
            }

            switch (section.Type)
            {
                case PdfSectionType.Text when !string.IsNullOrEmpty(section.TextContent):
                    column.Item().Text(section.TextContent).FontSize(10);
                    break;

                case PdfSectionType.KeyValue when section.KeyValues != null:
                    column.Item().Element(c => ComposeKeyValues(c, section.KeyValues));
                    break;

                case PdfSectionType.Table when section.TableData != null:
                    column.Item().Element(c => ComposeTableData(c, section.TableData));
                    break;
            }
        });
    }

    private static void ComposeKeyValues(IContainer container, List<PdfKeyValuePair> keyValues)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(150);
                columns.RelativeColumn();
            });

            foreach (var kv in keyValues)
            {
                table.Cell().Element(CellStyle).Text(kv.Key).Bold();
                table.Cell().Element(CellStyle).Text(kv.Value);
            }

            static IContainer CellStyle(IContainer c) => c.PaddingVertical(3);
        });
    }

    private static void ComposeTableData(IContainer container, PdfTableData tableData)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in tableData.Headers)
                    columns.RelativeColumn();
            });

            table.Header(header =>
            {
                foreach (var headerText in tableData.Headers)
                {
                    header.Cell()
                        .Background(Colors.Blue.Darken3)
                        .Padding(5)
                        .Text(headerText)
                        .FontColor(Colors.White)
                        .Bold()
                        .FontSize(9);
                }
            });

            var isAlternate = false;
            foreach (var row in tableData.Rows)
            {
                foreach (var cellValue in row)
                {
                    var cell = table.Cell()
                        .Background(isAlternate ? Colors.Grey.Lighten4 : Colors.White)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(4);

                    cell.Text(cellValue ?? "").FontSize(8);
                }
                isAlternate = !isAlternate;
            }
        });
    }

    private static void ComposeSummary(IContainer container, Dictionary<string, string> summaryData)
    {
        container.Background(Colors.Grey.Lighten4)
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(15)
            .Column(column =>
            {
                column.Item()
                    .PaddingBottom(10)
                    .Text("Summary")
                    .FontSize(12)
                    .Bold()
                    .FontColor(Colors.Blue.Darken2);

                column.Item().Row(row =>
                {
                    foreach (var kvp in summaryData)
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(kvp.Key).FontSize(9).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(kvp.Value).FontSize(12).Bold();
                        });
                    }
                });
            });
    }

    private static void ComposeTable<T>(IContainer container, string[] headers, List<T> items, Func<T, string[]> rowSelector)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in headers)
                    columns.RelativeColumn();
            });

            table.Header(header =>
            {
                foreach (var headerText in headers)
                {
                    header.Cell()
                        .Background(Colors.Blue.Darken3)
                        .Padding(5)
                        .Text(headerText)
                        .FontColor(Colors.White)
                        .Bold()
                        .FontSize(9);
                }
            });

            var isAlternate = false;
            foreach (var item in items)
            {
                var values = rowSelector(item);
                foreach (var cellValue in values)
                {
                    var cell = table.Cell()
                        .Background(isAlternate ? Colors.Grey.Lighten4 : Colors.White)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Padding(4);

                    cell.Text(cellValue ?? "").FontSize(8);
                }
                isAlternate = !isAlternate;
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem()
                    .Text($"© {DateTime.UtcNow.Year} Auction Platform")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Medium);

                row.RelativeItem()
                    .AlignRight()
                    .Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
            });
        });
    }
}

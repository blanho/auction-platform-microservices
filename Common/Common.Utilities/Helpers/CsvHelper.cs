using System.Text;

namespace Common.Utilities.Helpers;
public static class CsvHelper
{
    public static string EscapeValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('\n') || value.Contains('\r') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    public static string BuildLine(params object?[] values)
    {
        var formattedValues = values.Select(FormatValue);
        return string.Join(",", formattedValues);
    }

    public static byte[] GenerateCsv(string[] headers, IEnumerable<string[]> rows)
    {
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", headers.Select(EscapeValue)));

        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(",", row.Select(EscapeValue)));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    public static byte[] GenerateCsv<T>(
        IEnumerable<T> data,
        string[] headers,
        Func<T, object?[]> valueSelector)
    {
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", headers.Select(EscapeValue)));

        foreach (var item in data)
        {
            var values = valueSelector(item);
            csv.AppendLine(BuildLine(values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    public static IEnumerable<string[]> ParseCsv(string content, bool hasHeader = true)
    {
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var startIndex = hasHeader ? 1 : 0;

        for (var i = startIndex; i < lines.Length; i++)
        {
            yield return ParseCsvLine(lines[i]);
        }
    }

    public static string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentValue.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }
        }

        values.Add(currentValue.ToString());
        return values.ToArray();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => "",
            string s => EscapeValue(s),
            DateTime dt => dt.ToString("O"),
            DateTimeOffset dto => dto.ToString("O"),
            decimal d => d.ToString("F2"),
            double d => d.ToString("F2"),
            float f => f.ToString("F2"),
            _ => EscapeValue(value.ToString())
        };
    }
}

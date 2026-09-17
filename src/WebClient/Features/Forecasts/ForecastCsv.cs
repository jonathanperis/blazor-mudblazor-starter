using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

namespace WebClient.Features.Forecasts;

public static class ForecastCsv
{
    public const int MaxBytes = 1_048_576;
    public const int MaxRows = 1000;
    private const string Header = "Id,Date,TemperatureC,Summary";

    public static string Export(IEnumerable<WeatherForecast> records)
    {
        var builder = new StringBuilder(Header).Append('\n');
        foreach (var row in records)
        {
            var summary = row.Summary;
            // CSV consumers may be spreadsheets. Treat formula-like values as literal text.
            if (summary.Length > 0 && "=+-@\t\r".Contains(summary[0])) summary = "'" + summary;
            builder.Append(row.Id).Append(',')
                .Append(row.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',')
                .Append(row.TemperatureC?.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append('"').Append(summary.Replace("\"", "\"\"")).Append('"').Append('\n');
        }
        return builder.ToString();
    }

    public static List<WeatherForecast> Parse(string text)
    {
        if (Encoding.UTF8.GetByteCount(text) > MaxBytes) throw new FormatException("CSV must be at most 1 MiB.");
        var rows = ReadRows(text.TrimStart('\uFEFF'));
        if (rows.Count == 0 || string.Join(',', rows[0]) != Header) throw new FormatException($"Expected header: {Header}");
        if (rows.Count - 1 > MaxRows) throw new FormatException($"Import at most {MaxRows} records.");
        var result = new List<WeatherForecast>();
        var ids = new HashSet<Guid>();
        for (var i = 1; i < rows.Count; i++)
        {
            var fields = rows[i];
            if (fields.Length != 4 || !Guid.TryParse(fields[0], out var id) || !ids.Add(id)
                || !DateTime.TryParseExact(fields[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                || !int.TryParse(fields[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var temperature))
                throw new FormatException($"Row {i + 1}: expected unique ID, yyyy-MM-dd date, integer temperature, and summary.");
            var row = new WeatherForecast { Id = id, Date = date, TemperatureC = temperature, Summary = fields[3] };
            var errors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(row, new ValidationContext(row), errors, true))
                throw new FormatException($"Row {i + 1}: {string.Join(' ', errors.Select(e => e.ErrorMessage))}");
            result.Add(row);
        }
        return result;
    }

    private static List<string[]> ReadRows(string text)
    {
        var rows = new List<string[]>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var closedQuote = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c != '"') field.Append(c);
                else if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                else { quoted = false; closedQuote = true; }
            }
            else if (c == ',' || c is '\r' or '\n')
            {
                fields.Add(field.ToString()); field.Clear(); closedQuote = false;
                if (c != ',')
                {
                    rows.Add(fields.ToArray()); fields.Clear();
                    if (rows.Count > MaxRows + 1) throw new FormatException($"Import at most {MaxRows} records.");
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                }
            }
            else if (c == '"' && field.Length == 0 && !closedQuote) quoted = true;
            else if (c == '"' || closedQuote) throw new FormatException("Unexpected character after a quoted CSV field.");
            else field.Append(c);
        }
        if (quoted) throw new FormatException("CSV has an unclosed quoted field.");
        if (field.Length > 0 || fields.Count > 0 || closedQuote) { fields.Add(field.ToString()); rows.Add(fields.ToArray()); }
        return rows;
    }
}

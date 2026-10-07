using System.Globalization;

namespace WebClient.Shared.Features.Forecasts;

public static class ForecastData
{
    public const int MaxCount = 69420;
    public static readonly int[] Sizes = [100, 1000, 10000, MaxCount];
    private static readonly string[] Summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    public static List<WeatherForecast> Generate(int count, int seed = 42)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, MaxCount);
        var random = new Random(seed);
        var start = new DateTime(2026, 1, 1);
        var result = new List<WeatherForecast>(count);
        for (var i = 0; i < count; i++)
        {
            var bytes = new byte[16];
            random.NextBytes(bytes);
            result.Add(new WeatherForecast
            {
                Id = new Guid(bytes), Date = start.AddDays(i), TemperatureC = random.Next(-20, 55),
                Summary = Summaries[random.Next(Summaries.Length)]
            });
        }
        return result;
    }

    /// <summary>
    /// Matches summary, ID, temperature, or a date written in the given culture or as yyyy-MM-dd.
    /// The grid passes its current culture; the API receives the caller's culture explicitly.
    /// </summary>
    public static bool Matches(WeatherForecast row, string? search, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(search)) return true;
        culture ??= CultureInfo.CurrentCulture;
        string term = search;
        return Contains(row.Summary) || Contains(row.Id.ToString())
            || (row.Date is { } date && (Contains(date.ToString("d", culture)) || Contains(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))))
            || Contains(row.TemperatureC?.ToString(culture)) || Contains(row.TemperatureF?.ToString(culture));

        bool Contains(string? value) => value?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}

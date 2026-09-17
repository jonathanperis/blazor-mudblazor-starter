using System.Globalization;

namespace WebClient.Features.Forecasts;

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

    public static bool Matches(WeatherForecast row, string? search) => string.IsNullOrWhiteSpace(search)
        || row.Summary.Contains(search, StringComparison.OrdinalIgnoreCase)
        || row.Id.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)
        || (row.Date?.ToString("d", CultureInfo.CurrentCulture).Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
        || (row.TemperatureC?.ToString(CultureInfo.CurrentCulture).Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
        || (row.TemperatureF?.ToString(CultureInfo.CurrentCulture).Contains(search, StringComparison.OrdinalIgnoreCase) ?? false);
}

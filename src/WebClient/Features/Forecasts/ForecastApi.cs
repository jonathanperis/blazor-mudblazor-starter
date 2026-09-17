using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace WebClient.Features.Forecasts;

public sealed class ForecastQuery
{
    [Range(1, ForecastData.MaxCount)] public int Count { get; set; } = 1000;
    [Range(0, ForecastData.MaxCount)] public int Page { get; set; }
    [Range(1, 100)] public int PageSize { get; set; } = 25;
    [StringLength(120)] public string Search { get; set; } = "";
    [Range(0, 2000)] public int DelayMs { get; set; }
    public bool Fail { get; set; }
    public bool Descending { get; set; }
}

public sealed record ForecastPage(IReadOnlyList<WeatherForecast> Items, int Total);

public sealed class ForecastCatalog
{
    private readonly Lazy<List<WeatherForecast>> _records = new(() => ForecastData.Generate(ForecastData.MaxCount));

    public ForecastPage Read(ForecastQuery query)
    {
        Validator.ValidateObject(query, new ValidationContext(query), true);
        var matches = _records.Value.Take(query.Count).Where(row => ForecastData.Matches(row, query.Search));
        var sorted = query.Descending ? matches.OrderByDescending(row => row.Date) : matches.OrderBy(row => row.Date);
        var rows = sorted.ToList();
        return new ForecastPage(rows.Skip(query.Page * query.PageSize).Take(query.PageSize).ToList(), rows.Count);
    }
}

public sealed class ForecastApiClient(HttpClient http)
{
    public async Task<ForecastPage> ReadAsync(ForecastQuery query, CancellationToken cancellationToken)
    {
        var url = QueryHelpers.AddQueryString("api/forecasts", new Dictionary<string, string?>
        {
            ["count"] = query.Count.ToString(), ["page"] = query.Page.ToString(), ["pageSize"] = query.PageSize.ToString(),
            ["search"] = query.Search, ["delayMs"] = query.DelayMs.ToString(),
            ["fail"] = query.Fail.ToString(), ["descending"] = query.Descending.ToString()
        });
        return await http.GetFromJsonAsync<ForecastPage>(url, cancellationToken) ?? throw new HttpRequestException("Empty API response.");
    }
}

public static class ForecastEndpoints
{
    public static void MapForecastApi(this WebApplication app)
    {
        app.MapGet("/api/forecasts", async (int? count, int? page, int? pageSize, string? search, int? delayMs,
            bool? fail, bool? descending, ForecastCatalog catalog, CancellationToken cancellationToken) =>
        {
            var query = new ForecastQuery
            {
                Count = count ?? 1000, Page = page ?? 0, PageSize = pageSize ?? 25, Search = search ?? "",
                DelayMs = delayMs ?? 0, Fail = fail ?? false, Descending = descending ?? false
            };
            var errors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(query, new ValidationContext(query), errors, true))
                return Results.BadRequest(new { errors = errors.Select(e => e.ErrorMessage) });
            await Task.Delay(query.DelayMs, cancellationToken);
            return query.Fail ? Results.Problem("Simulated API failure. Turn off the failure switch to retry.", statusCode: 503)
                : Results.Ok(catalog.Read(query));
        });
    }
}

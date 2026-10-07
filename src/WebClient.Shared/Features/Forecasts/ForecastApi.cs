using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.Http.Json;
using WebClient.Shared.Features.Learning;

namespace WebClient.Shared.Features.Forecasts;

public sealed class ForecastQuery
{
    [Range(1, ForecastData.MaxCount)] public int Count { get; set; } = 1000;
    [Range(0, ForecastData.MaxCount)] public int Page { get; set; }
    [Range(1, 100)] public int PageSize { get; set; } = 25;
    [StringLength(120)] public string Search { get; set; } = "";
    [Range(0, 2000)] public int DelayMs { get; set; }
    public bool Fail { get; set; }
    public bool Descending { get; set; }
    /// <summary>
    /// The API cannot see the caller's UI culture, so the client sends it. Searching "02/01/2026" in
    /// pt-BR then matches the same row as "1/2/2026" in en-US.
    /// </summary>
    [AllowedValues("en-US", "pt-BR")] public string Culture { get; set; } = LearningCultures.Default;

    public ForecastQuery Clone() => (ForecastQuery)MemberwiseClone();

    /// <summary>Validation errors keyed by parameter name, in the shape of an HTTP validation problem.</summary>
    public Dictionary<string, string[]> Validate()
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(this, new ValidationContext(this), results, validateAllProperties: true);
        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty("").Select(member => (Member: JsonName(member), Message: result.ErrorMessage ?? "Invalid value.")))
            .GroupBy(error => error.Member, error => error.Message)
            .ToDictionary(group => group.Key, group => group.ToArray());

        static string JsonName(string member) => member.Length == 0 ? member : char.ToLowerInvariant(member[0]) + member[1..];
    }
}

public sealed record ForecastPage(IReadOnlyList<WeatherForecast> Items, int Total);

public sealed class ForecastCatalog
{
    private readonly Lazy<List<WeatherForecast>> _records = new(() => ForecastData.Generate(ForecastData.MaxCount));

    public ForecastPage Read(ForecastQuery query)
    {
        Validator.ValidateObject(query, new ValidationContext(query), true);
        var culture = CultureInfo.GetCultureInfo(query.Culture);
        // Generated dates ascend with the row index, so ordering is a direction rather than a sort.
        var rows = _records.Value.Take(query.Count);
        if (query.Descending) rows = rows.Reverse();
        var matches = rows.Where(row => ForecastData.Matches(row, query.Search, culture)).ToList();
        return new ForecastPage(matches.Skip(query.Page * query.PageSize).Take(query.PageSize).ToList(), matches.Count);
    }
}

public sealed class ForecastApiClient(HttpClient http)
{
    public async Task<ForecastPage> ReadAsync(ForecastQuery query, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            ["count"] = query.Count.ToString(CultureInfo.InvariantCulture),
            ["page"] = query.Page.ToString(CultureInfo.InvariantCulture),
            ["pageSize"] = query.PageSize.ToString(CultureInfo.InvariantCulture),
            ["search"] = query.Search,
            ["delayMs"] = query.DelayMs.ToString(CultureInfo.InvariantCulture),
            ["fail"] = query.Fail ? "true" : "false",
            ["descending"] = query.Descending ? "true" : "false",
            ["culture"] = LearningCultures.IsSupported(CultureInfo.CurrentCulture.Name) ? CultureInfo.CurrentCulture.Name : LearningCultures.Default
        };
        var url = "api/forecasts?" + string.Join('&', parameters.Select(pair => $"{pair.Key}={Uri.EscapeDataString(pair.Value)}"));
        return await http.GetFromJsonAsync<ForecastPage>(url, cancellationToken) ?? throw new HttpRequestException("Empty API response.");
    }
}

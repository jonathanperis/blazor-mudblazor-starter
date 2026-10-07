using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using WebClient.Shared.Features.Forecasts;

namespace WebClient.Shared.Features.StaticDemo;

/// <summary>
/// Answers <c>GET api/forecasts</c> inside the browser for the static GitHub Pages demo, which has no server.
/// The typed <see cref="ForecastApiClient"/> is unchanged: an <see cref="HttpMessageHandler"/> is the seam that
/// decides where a request goes. Validation, latency, failure and cancellation follow the server endpoint.
/// </summary>
public sealed class ForecastApiSimulator(ForecastCatalog catalog) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get || request.RequestUri?.AbsolutePath.EndsWith("/api/forecasts", StringComparison.Ordinal) != true)
            return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        var parameters = ParseQuery(request.RequestUri.Query);
        var query = new ForecastQuery();
        var errors = new Dictionary<string, string[]>();
        query.Count = ReadInt(parameters, "count", query.Count, errors);
        query.Page = ReadInt(parameters, "page", query.Page, errors);
        query.PageSize = ReadInt(parameters, "pageSize", query.PageSize, errors);
        query.DelayMs = ReadInt(parameters, "delayMs", query.DelayMs, errors);
        query.Search = parameters.GetValueOrDefault("search", "");
        query.Fail = parameters.GetValueOrDefault("fail") == "true";
        query.Descending = parameters.GetValueOrDefault("descending") == "true";
        query.Culture = parameters.GetValueOrDefault("culture", query.Culture);
        foreach (var (key, messages) in query.Validate()) errors[key] = messages;
        if (errors.Count > 0)
            return Respond(request, HttpStatusCode.BadRequest, new { title = "One or more validation errors occurred.", status = 400, errors });
        await Task.Delay(query.DelayMs, cancellationToken);
        return query.Fail
            ? Respond(request, HttpStatusCode.ServiceUnavailable, new { title = "Simulated API failure. Turn off the failure switch to retry.", status = 503 })
            : Respond(request, HttpStatusCode.OK, catalog.Read(query));
    }

    private static HttpResponseMessage Respond(HttpRequestMessage request, HttpStatusCode status, object body) =>
        new(status) { RequestMessage = request, Content = JsonContent.Create(body) };

    private static Dictionary<string, string> ParseQuery(string query) => query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split('=', 2).Select(part => Uri.UnescapeDataString(part.Replace('+', ' '))).ToArray())
        .GroupBy(parts => parts[0], StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Last().ElementAtOrDefault(1) ?? "", StringComparer.OrdinalIgnoreCase);

    private static int ReadInt(Dictionary<string, string> parameters, string name, int fallback, Dictionary<string, string[]> errors)
    {
        if (!parameters.TryGetValue(name, out var text)) return fallback;
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)) return value;
        errors[name] = [$"The value '{text}' is not a valid integer."];
        return fallback;
    }
}

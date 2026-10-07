using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;

namespace WebClient.Features.Forecasts;

public static class ForecastEndpoints
{
    public static void MapForecastApi(this WebApplication app)
    {
        app.MapGet("/api/forecasts", async (int? count, int? page, int? pageSize, string? search, int? delayMs,
            bool? fail, bool? descending, string? culture, ForecastCatalog catalog, CancellationToken cancellationToken) =>
        {
            var query = new ForecastQuery
            {
                Count = count ?? 1000, Page = page ?? 0, PageSize = pageSize ?? 25, Search = search ?? "",
                DelayMs = delayMs ?? 0, Fail = fail ?? false, Descending = descending ?? false,
                Culture = culture ?? LearningCultures.Default
            };
            var errors = query.Validate();
            if (errors.Count > 0) return Results.ValidationProblem(errors);
            await Task.Delay(query.DelayMs, cancellationToken);
            return query.Fail ? Results.Problem("Simulated API failure. Turn off the failure switch to retry.", statusCode: 503)
                : Results.Ok(catalog.Read(query));
        });
    }
}

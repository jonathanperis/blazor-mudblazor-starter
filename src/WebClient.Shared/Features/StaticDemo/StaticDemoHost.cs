using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WebClient.Shared.Components.StaticDemo;
using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.Notebook;

namespace WebClient.Shared.Features.StaticDemo;

/// <summary>
/// The static WebAssembly demo's host profile and browser services. The WebAssembly host runs them in the browser;
/// the build-time prerenderer renders the same pages with them, so both describe the demo identically.
/// </summary>
public static class StaticDemoHost
{
    public static LearningHost Profile { get; } = new()
    {
        Name = "Blazor WebAssembly",
        IsStaticDemo = true,
        ScopeLifetime = "browser tab",
        WorkLocation = "browser",
        NotebookStorage = "this browser's localStorage (the server host uses SQLite)",
        AuthPanel = typeof(BrowserAuthPanel),
        CulturePanel = typeof(BrowserCulturePanel),
        DiagnosticsPanel = typeof(BrowserDiagnosticsPanel),
        LabNotes = new Dictionary<string, string>
        {
            ["state"] = "there is no circuit. A scoped service lives as long as this browser tab's app instance: it survives navigation, and a reload resets both counts.",
            ["grid"] = "generation, filtering and sorting run on your device, so large datasets depend on its speed.",
            ["api"] = "GitHub Pages has no server. The same typed HttpClient sends each request, and an in-browser HttpMessageHandler answers it with the same validation, latency, failure and cancellation rules. Run locally to send real HTTP requests.",
            ["persistence"] = "notes are stored in this browser's localStorage instead of SQLite. Versions still detect conflicting edits from two tabs.",
            ["auth"] = "personas are browser-only and can be changed by anyone. Only a server can enforce the instructor policy.",
            ["localization"] = "the culture is saved in browser storage and applied before the .NET runtime starts, then a reload shows it.",
            ["files"] = "the CSV file is read and processed in your browser; nothing is uploaded.",
            ["observability"] = "logs go to the browser console. Health checks and server traces need the server host."
        }
    };

    /// <summary>Registers the labs with browser stand-ins for every server-only service.</summary>
    public static IServiceCollection AddStaticDemo(this IServiceCollection services, Uri baseAddress)
    {
        services.AddLearningLabs(Profile);
        services.AddScoped<INotebookStore, BrowserNotebookStore>();
        // The container owns the client and its in-browser handler, so both are disposed with the app's scope.
        services.AddKeyedScoped(nameof(ForecastApiClient), (provider, _) =>
            new HttpClient(new ForecastApiSimulator(provider.GetRequiredService<ForecastCatalog>()))
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(10)
            });
        services.AddScoped(provider => new ForecastApiClient(provider.GetRequiredKeyedService<HttpClient>(nameof(ForecastApiClient))));
        services.AddAuthorizationCore(options => options.AddPolicy(LearningPolicies.Instructor, policy => policy.RequireAuthenticatedUser().RequireRole("instructor")));
        services.AddScoped<AuthenticationStateProvider, DemoAuthenticationStateProvider>();
        return services;
    }
}

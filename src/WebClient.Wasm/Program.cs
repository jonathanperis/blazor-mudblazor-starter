using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using WebClient.Shared.Components.StaticDemo;
using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.Notebook;
using WebClient.Shared.Features.StaticDemo;
using WebClient.Wasm;

// Static WebAssembly host for the GitHub Pages live demo. There is no server: every service runs in the browser.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Logging.AddFilter("WebClient", LogLevel.Information);

builder.Services.AddLearningLabs(new LearningHost
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
});
builder.Services.AddScoped<INotebookStore, BrowserNotebookStore>();
// The container owns the client and its in-browser handler, so both are disposed with the app's scope.
builder.Services.AddKeyedScoped(nameof(ForecastApiClient), (services, _) =>
    new HttpClient(new ForecastApiSimulator(services.GetRequiredService<ForecastCatalog>()))
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
        Timeout = TimeSpan.FromSeconds(10)
    });
builder.Services.AddScoped(services => new ForecastApiClient(services.GetRequiredKeyedService<HttpClient>(nameof(ForecastApiClient))));
builder.Services.AddAuthorizationCore(options => options.AddPolicy(LearningPolicies.Instructor, policy => policy.RequireAuthenticatedUser().RequireRole("instructor")));
builder.Services.AddScoped<AuthenticationStateProvider, DemoAuthenticationStateProvider>();

var host = builder.Build();
// Apply the stored culture before the first render; changing it later requires a reload, as on the server.
var js = host.Services.GetRequiredService<IJSRuntime>();
var stored = await js.InvokeAsync<string?>("learningCulture.get");
var culture = CultureInfo.GetCultureInfo(LearningCultures.IsSupported(stored) ? stored! : LearningCultures.Default);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
await js.InvokeVoidAsync("learningCulture.applyDocumentLanguage", culture.TwoLetterISOLanguageName);
await host.RunAsync();

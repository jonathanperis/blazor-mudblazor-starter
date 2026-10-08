using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using WebClient.Prerender;
using WebClient.Shared.Features.Gallery;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.Notebook;
using WebClient.Shared.Features.StaticDemo;

// Renders every page of the static demo into the published wwwroot, so GitHub Pages answers a deep link with the
// page itself: readable before .NET starts, and with its own title and description for search engines and link
// previews. Blazor replaces the prerendered content with the interactive app on its first render.
//
// Usage: dotnet run --project src/WebClient.Prerender -- <published wwwroot> [public demo URL]
if (args.Length is < 1 or > 2)
{
    Console.Error.WriteLine("Usage: WebClient.Prerender <published wwwroot> [public demo URL]");
    return 2;
}
var wwwroot = Path.GetFullPath(args[0]);
var publicUrl = args.Length == 2 ? args[1] : "https://jonathanperis.github.io/blazor-mudblazor-starter/demo/";
var indexPath = Path.Join(wwwroot, "index.html");
if (!File.Exists(indexPath) || !publicUrl.EndsWith('/'))
{
    Console.Error.WriteLine($"Expected a published index.html in {wwwroot} and a public URL ending in /.");
    return 2;
}
// Pages render in the demo's default culture, whatever the build machine's is; the app re-renders in the visitor's.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture =
    CultureInfo.GetCultureInfo(LearningCultures.Default);
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
var template = File.ReadAllText(indexPath);
// The unrendered shell stays available: 404.html sends routes without a prerendered page (unknown ones) to it.
var shellPath = Path.Join(wwwroot, "app.html");
if (!File.Exists(shellPath)) File.WriteAllText(shellPath, template, utf8);
else template = File.ReadAllText(shellPath);

const string BaseUri = "http://demo.invalid/";
var settleTime = TimeSpan.FromSeconds(3);
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddSimpleConsole(options => options.SingleLine = true).SetMinimumLevel(LogLevel.Warning));
services.AddStaticDemo(new Uri(BaseUri));
services.AddScoped<StaticNavigationManager>();
services.AddScoped<NavigationManager>(provider => provider.GetRequiredService<StaticNavigationManager>());
services.AddScoped<IJSRuntime, StaticJSRuntime>();
services.AddScoped<INotebookStore, PendingNotebookStore>();
services.AddScoped<CollectingErrorBoundaryLogger>();
services.AddScoped<IErrorBoundaryLogger>(provider => provider.GetRequiredService<CollectingErrorBoundaryLogger>());
await using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

var routes = typeof(LabCatalog).Assembly.GetExportedTypes()
    .SelectMany(type => type.GetCustomAttributes<RouteAttribute>().Select(route => (type, route.Template)))
    .Where(page => !page.Template.Contains('{') && page.Template is not ("/Error" or "/not-found"))
    .OrderBy(page => page.Template, StringComparer.Ordinal)
    .ToList();
var directories = routes.Select(page => page.Template.TrimStart('/')).Where(path => path.Length > 0)
    .SelectMany(path => Enumerable.Range(1, path.Count(c => c == '/')).Select(depth => string.Join('/', path.Split('/').Take(depth))))
    .ToHashSet(StringComparer.Ordinal);

var darkTheme = await RenderAsync(null, typeof(DarkTheme));
var failures = new List<string>();
var bytes = 0L;
foreach (var (page, route) in routes)
{
    var path = route.TrimStart('/');
    await using var scope = provider.CreateAsyncScope();
    scope.ServiceProvider.GetRequiredService<StaticNavigationManager>().Start(BaseUri, BaseUri + path);
    var (body, head, settled) = await RenderPageAsync(scope.ServiceProvider, page);
    if (scope.ServiceProvider.GetRequiredService<CollectingErrorBoundaryLogger>().Errors is [var error, ..])
    {
        failures.Add($"{route}: an error boundary caught {error.GetType().Name}: {error.Message}");
        continue;
    }
    // Only a page waiting on browser data may be captured before it settles; anything else is slow or stuck.
    if (!settled && !((PendingNotebookStore)scope.ServiceProvider.GetRequiredService<INotebookStore>()).Waiting)
    {
        failures.Add($"{route}: still rendering after {settleTime.TotalSeconds:0} s without waiting on browser data");
        continue;
    }
    var html = Shell.Compose(template, publicUrl + path, body, head, Description(route), darkTheme);
    // "/labs/api" is served from labs/api.html. A route that is also a folder ("/labs") gets labs/index.html too,
    // because a static host may answer "/labs" from either one.
    var files = path.Length == 0 ? ["index.html"] : directories.Contains(path) ? new[] { $"{path}.html", $"{path}/index.html" } : [$"{path}.html"];
    foreach (var target in files.Select(file => Path.Join(wwwroot, file)))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        await File.WriteAllTextAsync(target, html, utf8);
        bytes += utf8.GetByteCount(html);
    }
}

if (failures.Count > 0)
{
    Console.Error.WriteLine($"Prerendering failed:{Environment.NewLine}  {string.Join($"{Environment.NewLine}  ", failures)}");
    return 1;
}
Console.WriteLine($"Prerendered {routes.Count} routes into {wwwroot} ({bytes / 1_048_576.0:F1} MiB of HTML)");
return 0;

async Task<(string Body, string Head, bool Settled)> RenderPageAsync(IServiceProvider scoped, Type page)
{
    await using var renderer = new HtmlRenderer(scoped, loggerFactory);
    return await renderer.Dispatcher.InvokeAsync(async () =>
    {
        var body = renderer.BeginRenderingComponent<PrerenderedPage>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Page"] = page }));
        // Most pages settle at once. One that waits on the browser (the notebook) is captured in its loading state.
        var settled = await Task.WhenAny(body.QuiescenceTask, Task.Delay(settleTime)) == body.QuiescenceTask;
        if (settled) await body.QuiescenceTask;
        // HeadOutlet shows what the page's PageTitle and HeadContent supplied while the body rendered.
        // Rendering it does not wait for the body: a pending page would keep the renderer from ever settling.
        var head = renderer.BeginRenderingComponent<HeadOutlet>();
        return (body.ToHtmlString(), head.ToHtmlString(), settled);
    });
}

async Task<string> RenderAsync(string? route, Type component)
{
    await using var scope = provider.CreateAsyncScope();
    scope.ServiceProvider.GetRequiredService<StaticNavigationManager>().Start(BaseUri, BaseUri + route);
    await using var renderer = new HtmlRenderer(scope.ServiceProvider, loggerFactory);
    return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync(component)).ToHtmlString());
}

string Description(string route) =>
    LabCatalog.All.FirstOrDefault(lab => lab.Route == route)?.Objective
    ?? GalleryCatalog.Components.FirstOrDefault(entry => entry.Route == route)?.Summary
    ?? GalleryCatalog.Samples.FirstOrDefault(sample => sample.Route == route)?.Summary
    ?? "Live Blazor WebAssembly demo of the Blazor learning sandbox: nine labs, a MudBlazor component gallery and page samples. Runs entirely in the browser.";

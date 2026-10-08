using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.StaticDemo;
using WebClient.Wasm;

// Static WebAssembly host for the GitHub Pages live demo. There is no server: every service runs in the browser.
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Logging.AddFilter("WebClient", LogLevel.Information);

builder.Services.AddStaticDemo(new Uri(builder.HostEnvironment.BaseAddress));

var host = builder.Build();
// Apply the stored culture before the first render; changing it later requires a reload, as on the server.
var js = host.Services.GetRequiredService<IJSRuntime>();
var stored = await js.InvokeAsync<string?>("learningCulture.get");
var culture = CultureInfo.GetCultureInfo(LearningCultures.IsSupported(stored) ? stored! : LearningCultures.Default);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
await js.InvokeVoidAsync("learningCulture.applyDocumentLanguage", culture.TwoLetterISOLanguageName);
await host.RunAsync();

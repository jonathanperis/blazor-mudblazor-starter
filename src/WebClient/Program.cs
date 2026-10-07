using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using WebClient.Components;
using WebClient.Components.Server;
using WebClient.Features.Forecasts;
using WebClient.Features.Identity;
using WebClient.Features.Notebook;
using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.Notebook;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddLearningLabs(new LearningHost
{
    Name = "Blazor Server",
    IsStaticDemo = false,
    ScopeLifetime = "circuit",
    WorkLocation = "server",
    NotebookStorage = "SQLite on the server, scoped to this browser by a protected workspace cookie",
    AuthPanel = typeof(ServerAuthPanel),
    CulturePanel = typeof(ServerCulturePanel),
    DiagnosticsPanel = typeof(ServerDiagnosticsPanel)
});
// The culture changes only when the learner chooses one (the culture cookie), as in the WebAssembly host. Following
// Accept-Language would mix translated MudBlazor labels into an English interface.
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture(LearningCultures.Default).AddSupportedCultures(LearningCultures.Supported).AddSupportedUICultures(LearningCultures.Supported);
    options.RequestCultureProviders = [new Microsoft.AspNetCore.Localization.CookieRequestCultureProvider()];
});
// The security-header middleware below sends X-Frame-Options: DENY for every response.
builder.Services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);
builder.Services.Configure<FormOptions>(options => { options.ValueLengthLimit = 4096; options.ValueCountLimit = 16; });
var apiBase = builder.Configuration["Learning:ApiBaseUrl"] ?? "http://127.0.0.1:5000/";
builder.Services.AddHttpClient<ForecastApiClient>(client => { client.BaseAddress = new Uri(apiBase); client.Timeout = TimeSpan.FromSeconds(10); });
builder.Services.AddHttpClient("LearningApi", client => { client.BaseAddress = new Uri(apiBase); client.Timeout = TimeSpan.FromSeconds(10); });

var dataDirectory = Path.GetFullPath(builder.Configuration["Learning:DataDirectory"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
Directory.CreateDirectory(dataDirectory);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDirectory, "keys"))).SetApplicationName("BlazorLearningSandbox");
builder.Services.AddDbContextFactory<NotebookDb>(options => options.UseSqlite($"Data Source={Path.Combine(dataDirectory, "notebook.db")}"));
builder.Services.AddScoped<LearnerWorkspace>();
builder.Services.AddScoped<INotebookStore, NotebookService>();

var demoAuth = new DemoAuthOptions(builder.Configuration.GetValue<bool?>("Learning:EnableDemoAuth") ?? builder.Environment.IsDevelopment());
builder.Services.AddSingleton(demoAuth);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "learning.demo";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    options.Events.OnValidatePrincipal = context => { if (!demoAuth.Enabled) context.RejectPrincipal(); return Task.CompletedTask; };
});
builder.Services.AddAuthorization(options => options.AddPolicy(LearningPolicies.Instructor, policy => policy.RequireAuthenticatedUser().RequireRole("instructor")));
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHealthChecks().AddCheck<NotebookHealthCheck>("notebook", tags: ["ready"]);
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
    builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<NotebookDb>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}
// Behind a TLS-terminating proxy, set ASPNETCORE_FORWARDEDHEADERS_ENABLED=true so Request.IsHttps and client
// addresses reflect the original request; cookies are then marked Secure. See the deployment guide.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
// Configure HTTPS redirection only when the hosting environment supplies a TLS endpoint.
if (app.Configuration["HTTPS_PORT"] is not null) app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    // Baseline response headers: no framing (clickjacking) and no MIME sniffing. Interactive pages also get
    // Blazor's frame-ancestors policy below. A full Content-Security-Policy needs per-page tuning for MudBlazor.
    var headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next(context);
});
app.UseRequestLocalization();
app.Use(LearnerWorkspace.EstablishAsync);
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapHealthChecks("/healthz", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/healthz/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapForecastApi();
app.MapLearningIdentity();
app.MapGet("/api/diagnostics", (ILogger<Program> logger) =>
{
    var traceId = Activity.Current?.TraceId.ToString() ?? "unavailable";
    logger.LogInformation("Learning diagnostic request {TraceId}", traceId);
    return Results.Ok(new { traceId, message = "Find this trace ID in the server console.", utc = DateTimeOffset.UtcNow });
});
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(options => options.ContentSecurityFrameAncestorsPolicy = "'none'")
    .AddAdditionalAssemblies(typeof(LabCatalog).Assembly);
app.Run();

public partial class Program;

using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using MudBlazor.Translations;
using WebClient.Components;
using WebClient.Features.Forecasts;
using WebClient.Features.Identity;
using WebClient.Features.Learning;
using WebClient.Features.Notebook;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices(options => options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight);
builder.Services.AddMudTranslations();
builder.Services.AddLocalization();
builder.Services.Configure<RequestLocalizationOptions>(options => options
    .SetDefaultCulture("en-US").AddSupportedCultures(DemoIdentity.Cultures).AddSupportedUICultures(DemoIdentity.Cultures));
builder.Services.Configure<FormOptions>(options => { options.ValueLengthLimit = 4096; options.ValueCountLimit = 16; });
builder.Services.AddScoped<CircuitCounter>();
builder.Services.AddScoped<UiPreferences>();
builder.Services.AddSingleton<ForecastCatalog>();
var apiBase = builder.Configuration["Learning:ApiBaseUrl"] ?? "http://127.0.0.1:5000/";
builder.Services.AddHttpClient<ForecastApiClient>(client => { client.BaseAddress = new Uri(apiBase); client.Timeout = TimeSpan.FromSeconds(10); });
builder.Services.AddHttpClient("LearningApi", client => { client.BaseAddress = new Uri(apiBase); client.Timeout = TimeSpan.FromSeconds(10); });

var dataDirectory = Path.GetFullPath(builder.Configuration["Learning:DataDirectory"] ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
Directory.CreateDirectory(dataDirectory);
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(dataDirectory, "keys"))).SetApplicationName("BlazorLearningSandbox");
builder.Services.AddDbContextFactory<NotebookDb>(options => options.UseSqlite($"Data Source={Path.Combine(dataDirectory, "notebook.db")}"));
builder.Services.AddScoped<LearnerWorkspace>();
builder.Services.AddScoped<NotebookService>();

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
builder.Services.AddAuthorization(options => options.AddPolicy(DemoIdentity.InstructorPolicy, policy => policy.RequireAuthenticatedUser().RequireRole("instructor")));
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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
// Azure terminates TLS at its front end. Use the HTTPS launch profile for local TLS.
if (app.Configuration["HTTPS_PORT"] is not null) app.UseHttpsRedirection();
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
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

public partial class Program;

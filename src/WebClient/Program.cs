using MudBlazor;
using MudBlazor.Services;
using MudBlazor.Translations;
using WebClient.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddMudTranslations();

// Add health checks
builder.Services.AddHealthChecks();

// Send all exceptions to the console
MudGlobal.UnhandledExceptionHandler = Console.WriteLine;

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapHealthChecks("/healthz");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

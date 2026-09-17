using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebClient.Tests;

public sealed class SandboxFactory : WebApplicationFactory<Program>
{
    private readonly string _directory = Path.Combine(AppContext.BaseDirectory, "test-data", Guid.NewGuid().ToString("N"));
    public bool DemoAuthEnabled { get; init; } = true;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Learning:DataDirectory", _directory);
        builder.UseSetting("Learning:EnableDemoAuth", DemoAuthEnabled.ToString());
        builder.UseSetting("APPLICATIONINSIGHTS_CONNECTION_STRING", "");
    }

    public HttpClient BrowserClient() => CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directory)) Directory.Delete(_directory, recursive: true);
    }
}

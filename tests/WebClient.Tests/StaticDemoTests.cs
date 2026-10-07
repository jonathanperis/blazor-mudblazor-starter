using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.Notebook;
using WebClient.Shared.Features.StaticDemo;

namespace WebClient.Tests;

/// <summary>The browser stand-ins used by the GitHub Pages WebAssembly demo keep the server contracts.</summary>
public sealed class StaticDemoTests : BunitContext
{
    private static HttpClient Client() => new(new ForecastApiSimulator(new ForecastCatalog())) { BaseAddress = new Uri("https://example.test/demo/") };

    [Fact]
    public async Task Simulated_api_pages_validates_fails_and_cancels_like_the_endpoint()
    {
        using var http = Client();
        var page = await http.GetFromJsonAsync<ForecastPage>("api/forecasts?count=10&page=1&pageSize=3");
        Assert.Equal(10, page!.Total);
        Assert.Equal(ForecastData.Generate(10).Skip(3).Take(3).Select(r => r.Id), page.Items.Select(r => r.Id));
        Assert.Equal(HttpStatusCode.BadRequest, (await http.GetAsync("api/forecasts?count=1000000")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.GetAsync("api/forecasts?count=ten")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await http.GetAsync("api/forecasts?culture=fr-FR")).StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await http.GetAsync("api/forecasts?fail=true")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await http.GetAsync("api/other")).StatusCode);
        using var cancellation = new CancellationTokenSource(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => http.GetAsync("api/forecasts?delayMs=2000", cancellation.Token));
    }

    [Fact]
    public async Task Typed_client_reaches_the_simulator_with_an_encoded_search()
    {
        var client = new ForecastApiClient(Client());
        var page = await client.ReadAsync(new ForecastQuery { Count = 100, Search = "2026-01-02" }, CancellationToken.None);
        Assert.Equal(new DateTime(2026, 1, 2), Assert.Single(page.Items).Date);
    }

    [Fact]
    public async Task Browser_notebook_maps_storage_outcomes_to_the_shared_contract()
    {
        var store = new BrowserNotebookStore(Services.GetRequiredService<IJSRuntime>());
        var id = Guid.NewGuid();
        var version = Guid.NewGuid();
        JSInterop.Setup<BrowserNote[]?>("learningNotebook.list").SetResult([]);
        JSInterop.Setup<string>("learningNotebook.insert", _ => true).SetResult("limit");
        JSInterop.Setup<string>("learningNotebook.update", _ => true).SetResult("conflict");
        JSInterop.Setup<string>("learningNotebook.remove", _ => true).SetResult("ok");
        await Assert.ThrowsAsync<NotebookLimitException>(() => store.SaveAsync(new NoteDraft { Title = "One too many" }));
        await Assert.ThrowsAsync<NotebookConflictException>(() => store.SaveAsync(new NoteDraft { Title = "Stale" }, new NoteSnapshot(id, "Old", "", version)));
        await store.DeleteAsync(new NoteSnapshot(id, "Old", "", version));
        var removal = Assert.Single(JSInterop.Invocations["learningNotebook.remove"]);
        Assert.Equal(new object?[] { id.ToString(), version.ToString() }, removal.Arguments);
        await Assert.ThrowsAnyAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => store.SaveAsync(new NoteDraft { Title = "" }));
    }

    [Fact]
    public async Task Browser_notebook_reports_unavailable_storage()
    {
        JSInterop.Setup<BrowserNote[]?>("learningNotebook.list").SetException(new JSException("Browser storage is unavailable."));
        var store = new BrowserNotebookStore(Services.GetRequiredService<IJSRuntime>());
        await Assert.ThrowsAsync<NotebookUnavailableException>(() => store.ListAsync());
    }

    [Fact]
    public async Task Browser_personas_change_identity_but_reject_unknown_roles()
    {
        var provider = new DemoAuthenticationStateProvider();
        Assert.False((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);
        provider.SignIn("instructor");
        var user = (await provider.GetAuthenticationStateAsync()).User;
        Assert.True(user.IsInRole("instructor"));
        Assert.Equal("Demo instructor", user.FindFirstValue(ClaimTypes.Name));
        Assert.Throws<ArgumentOutOfRangeException>(() => provider.SignIn("admin"));
        provider.SignOut();
        Assert.False((await provider.GetAuthenticationStateAsync()).User.Identity!.IsAuthenticated);
    }

    [Fact]
    public void Every_lab_links_to_existing_source_and_prerequisites()
    {
        var root = AppContext.BaseDirectory;
        while (!File.Exists(Path.Join(root, "WebClient.sln"))) root = Path.GetDirectoryName(root)!;
        foreach (var lab in LabCatalog.All)
        {
            Assert.True(File.Exists(Path.Join(root, lab.Source)), $"Missing source for {lab.Slug}: {lab.Source}");
            Assert.Contains($"@page \"{lab.Route}\"", File.ReadAllText(Path.Join(root, lab.Source)));
            Assert.All(lab.Prerequisites, slug => LabCatalog.Get(slug));
        }
    }
}

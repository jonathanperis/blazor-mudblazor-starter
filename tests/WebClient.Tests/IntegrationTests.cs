using System.Net;
using System.Net.Http.Json;
using AngleSharp.Html.Parser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebClient.Features.Forecasts;
using WebClient.Features.Learning;
using WebClient.Features.Notebook;

namespace WebClient.Tests;

public sealed class IntegrationTests
{
    [Fact]
    public async Task All_labs_prerender_headings_and_health_checks_pass()
    {
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        foreach (var route in LabCatalog.All.Select(lab => lab.Route).Concat(["/", "/labs"]))
        {
            var response = await client.GetAsync(route);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
            Assert.NotNull(document.QuerySelector("main h1"));
        }
        Assert.Equal("Healthy", await client.GetStringAsync("/healthz"));
        Assert.Equal("Healthy", await client.GetStringAsync("/healthz/ready"));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/not-a-lab")).StatusCode);
    }

    [Fact]
    public async Task Api_pages_validates_simulates_failure_and_cancels()
    {
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var result = await client.GetFromJsonAsync<ForecastPage>("/api/forecasts?count=10&page=1&pageSize=3");
        Assert.Equal(10, result!.Total);
        Assert.Equal(ForecastData.Generate(10).Skip(3).Take(3).Select(r => r.Id), result.Items.Select(r => r.Id));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/forecasts?count=1000000")).StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/api/forecasts?fail=true")).StatusCode);
        using var cancellation = new CancellationTokenSource(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetAsync("/api/forecasts?delayMs=2000", cancellation.Token));
    }

    [Fact]
    public async Task Demo_identity_enforces_antiforgery_and_server_policy()
    {
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/instructor")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/auth/demo", new FormUrlEncodedContent(new Dictionary<string, string> { ["persona"] = "instructor" }))).StatusCode);
        foreach (var (persona, expected) in new[] { ("student", HttpStatusCode.Forbidden), ("instructor", HttpStatusCode.OK) })
        {
            var token = await Token(client, "/labs/auth");
            Assert.Equal(HttpStatusCode.Redirect, (await Post(client, "/auth/demo", token, "persona", persona)).StatusCode);
            Assert.Equal(expected, (await client.GetAsync("/api/instructor")).StatusCode);
        }
        Assert.Equal(HttpStatusCode.Redirect, (await Post(client, "/auth/logout", await Token(client, "/labs/auth"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/instructor")).StatusCode);
    }

    [Fact]
    public async Task Disabled_demo_login_cannot_issue_an_identity()
    {
        await using var factory = new SandboxFactory { DemoAuthEnabled = false };
        using var client = factory.BrowserClient();
        Assert.Equal(HttpStatusCode.NotFound, (await Post(client, "/auth/demo", "", "persona", "instructor")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/instructor")).StatusCode);
    }

    [Fact]
    public async Task Culture_cookie_changes_server_formatting_and_translated_content()
    {
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var token = await Token(client, "/labs/localization");
        Assert.Equal(HttpStatusCode.Redirect, (await Post(client, "/culture", token, "culture", "pt-BR")).StatusCode);
        var document = new HtmlParser().ParseDocument(await client.GetStringAsync("/labs/localization"));
        Assert.Equal("pt", document.DocumentElement.GetAttribute("lang"));
        Assert.Contains("Bem-vindo", document.Body!.TextContent);
        Assert.Contains("1.234,56", document.Body.TextContent);
    }

    [Fact]
    public async Task Notebook_migrates_isolates_workspaces_and_rejects_stale_writes()
    {
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var dbFactory = factory.Services.GetRequiredService<IDbContextFactory<NotebookDb>>();
        var alice = new NotebookService(dbFactory, new LearnerWorkspace("alice"));
        var bob = new NotebookService(dbFactory, new LearnerWorkspace("bob"));
        await alice.SaveAsync(new NoteDraft { Title = "Original" });
        var original = Assert.Single(await alice.ListAsync());
        Assert.Empty(await bob.ListAsync());
        await bob.SaveAsync(new NoteDraft { Title = "Bob's note" });
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => bob.SaveAsync(new NoteDraft { Title = "Cross-workspace write" }, original));
        await alice.SaveAsync(new NoteDraft { Title = "Updated" }, original);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => alice.SaveAsync(new NoteDraft { Title = "Stale" }, original));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => alice.DeleteAsync(original));
        Assert.Equal("Updated", Assert.Single(await alice.ListAsync()).Title);
        await alice.ResetAsync();
        Assert.Empty(await alice.ListAsync());
        Assert.Single(await bob.ListAsync());
    }

    private static async Task<string> Token(HttpClient client, string route)
    {
        var document = new HtmlParser().ParseDocument(await client.GetStringAsync(route));
        return document.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;
    }

    private static Task<HttpResponseMessage> Post(HttpClient client, string route, string token, string? name = null, string? value = null)
    {
        var form = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        if (name is not null) form[name] = value!;
        return client.PostAsync(route, new FormUrlEncodedContent(form));
    }
}

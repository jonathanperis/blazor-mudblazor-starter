using System.Net;
using System.Net.Http.Json;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebClient.Features.Notebook;
using WebClient.Shared.Features.Forecasts;
using WebClient.Shared.Features.Learning;
using WebClient.Shared.Features.Notebook;

namespace WebClient.Tests;

public sealed class IntegrationTests
{
    [Fact]
    public async Task All_labs_prerender_headings_and_health_checks_pass()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var gallery = WebClient.Shared.Features.Gallery.GalleryCatalog.Components.Select(entry => entry.Route)
            .Concat(WebClient.Shared.Features.Gallery.GalleryCatalog.Samples.Select(sample => sample.Route));
        foreach (var route in LabCatalog.All.Select(lab => lab.Route).Concat(["/", "/labs", "/components", "/samples"]).Concat(gallery))
        {
            var response = await client.GetAsync(route, cancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"{route} returned {response.StatusCode}");
            var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(cancellationToken));
            Assert.NotNull(document.QuerySelector("main h1"));
        }
        Assert.Equal("Healthy", await client.GetStringAsync("/healthz", cancellationToken));
        Assert.Equal("Healthy", await client.GetStringAsync("/healthz/ready", cancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/not-a-lab", cancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Pages_send_baseline_security_headers_and_a_path_preserving_skip_link()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var response = await client.GetAsync("/labs/api", cancellationToken);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        var document = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.EndsWith("/labs/api#main-content", document.QuerySelector("a.skip-link")!.GetAttribute("href"));
        Assert.DoesNotContain(document.QuerySelectorAll("a[href^='/']"), link => !link.GetAttribute("href")!.StartsWith("//"));
    }

    [Fact]
    public async Task Workspace_cookie_is_page_only_sliding_and_reissued_when_tampered()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        foreach (var route in new[] { "/healthz", "/api/forecasts?count=1", "/favicon.png" })
            Assert.False((await client.GetAsync(route, cancellationToken)).Headers.Contains("Set-Cookie"), $"{route} set a cookie");
        var protector = factory.Services.GetRequiredService<IDataProtectionProvider>().CreateProtector("learning.workspace");
        var first = WorkspaceCookie(await client.GetAsync("/labs/persistence", cancellationToken));
        var second = WorkspaceCookie(await client.GetAsync("/labs/persistence", cancellationToken));
        Assert.Contains("max-age=2592000", first.Header);
        Assert.Equal(protector.Unprotect(first.Value), protector.Unprotect(second.Value));
        using var tampered = new HttpRequestMessage(HttpMethod.Get, "/labs/persistence");
        tampered.Headers.Add("Cookie", "learning.workspace=tampered");
        using var fresh = factory.BrowserClient();
        var replacement = WorkspaceCookie(await fresh.SendAsync(tampered, cancellationToken));
        Assert.True(Guid.TryParseExact(protector.Unprotect(replacement.Value), "N", out _));
    }

    [Fact]
    public async Task Api_validation_returns_problem_details_and_honors_the_culture()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var invalid = await client.GetAsync("/api/forecasts?culture=fr-FR&pageSize=500", cancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType!.MediaType);
        var problem = await invalid.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>(cancellationToken);
        Assert.True(problem!["errors"].TryGetProperty("culture", out _));
        Assert.True(problem["errors"].TryGetProperty("pageSize", out _));
        var match = await client.GetFromJsonAsync<ForecastPage>("/api/forecasts?count=100&culture=pt-BR&search=02%2F01%2F2026", cancellationToken);
        Assert.Equal(new DateTime(2026, 1, 2), Assert.Single(match!.Items).Date);
    }

    [Fact]
    public async Task Api_pages_validates_simulates_failure_and_cancels()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var result = await client.GetFromJsonAsync<ForecastPage>("/api/forecasts?count=10&page=1&pageSize=3", cancellationToken);
        Assert.Equal(10, result!.Total);
        Assert.Equal(ForecastData.Generate(10).Skip(3).Take(3).Select(r => r.Id), result.Items.Select(r => r.Id));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/forecasts?count=1000000", cancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, (await client.GetAsync("/api/forecasts?fail=true", cancellationToken)).StatusCode);
        using var cancellation = new CancellationTokenSource(50);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetAsync("/api/forecasts?delayMs=2000", cancellation.Token));
    }

    [Fact]
    public async Task Demo_identity_enforces_antiforgery_and_server_policy()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/instructor", cancellationToken)).StatusCode);
        using var withoutToken = new FormUrlEncodedContent(new Dictionary<string, string> { ["persona"] = "instructor" });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/auth/demo", withoutToken, cancellationToken)).StatusCode);
        foreach (var (persona, expected) in new[] { ("student", HttpStatusCode.Forbidden), ("instructor", HttpStatusCode.OK) })
        {
            var token = await Token(client, "/labs/auth");
            Assert.Equal(HttpStatusCode.Redirect, (await Post(client, "/auth/demo", token, "persona", persona)).StatusCode);
            Assert.Equal(expected, (await client.GetAsync("/api/instructor", cancellationToken)).StatusCode);
        }
        Assert.Equal(HttpStatusCode.Redirect, (await Post(client, "/auth/logout", await Token(client, "/labs/auth"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/instructor", cancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Disabled_demo_login_cannot_issue_an_identity()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory { DemoAuthEnabled = false };
        using var client = factory.BrowserClient();
        Assert.Equal(HttpStatusCode.NotFound, (await Post(client, "/auth/demo", "", "persona", "instructor")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/instructor", cancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Culture_cookie_changes_server_formatting_and_translated_content()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var token = await Token(client, "/labs/localization");
        Assert.Equal(HttpStatusCode.Redirect, (await Post(client, "/culture", token, "culture", "pt-BR")).StatusCode);
        var document = new HtmlParser().ParseDocument(await client.GetStringAsync("/labs/localization", cancellationToken));
        Assert.Equal("pt", document.DocumentElement.GetAttribute("lang"));
        Assert.Contains("Bem-vindo", document.Body!.TextContent);
        Assert.Contains("1.234,56", document.Body.TextContent);
    }

    [Fact]
    public async Task Notebook_migrates_isolates_workspaces_and_rejects_stale_writes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = new SandboxFactory();
        using var client = factory.BrowserClient();
        var dbFactory = factory.Services.GetRequiredService<IDbContextFactory<NotebookDb>>();
        var alice = new NotebookService(dbFactory, new LearnerWorkspace("alice"));
        var bob = new NotebookService(dbFactory, new LearnerWorkspace("bob"));
        await alice.SaveAsync(new NoteDraft { Title = "Original" }, cancellationToken: cancellationToken);
        var original = Assert.Single(await alice.ListAsync(cancellationToken));
        Assert.Empty(await bob.ListAsync(cancellationToken));
        await bob.SaveAsync(new NoteDraft { Title = "Bob's note" }, cancellationToken: cancellationToken);
        await Assert.ThrowsAsync<NotebookConflictException>(() => bob.SaveAsync(new NoteDraft { Title = "Cross-workspace write" }, original, cancellationToken));
        await alice.SaveAsync(new NoteDraft { Title = "Updated" }, original, cancellationToken);
        await Assert.ThrowsAsync<NotebookConflictException>(() => alice.SaveAsync(new NoteDraft { Title = "Stale" }, original, cancellationToken));
        await Assert.ThrowsAsync<NotebookConflictException>(() => alice.DeleteAsync(original, cancellationToken));
        Assert.Equal("Updated", Assert.Single(await alice.ListAsync(cancellationToken)).Title);
        await alice.ResetAsync(cancellationToken);
        Assert.Empty(await alice.ListAsync(cancellationToken));
        Assert.Single(await bob.ListAsync(cancellationToken));
        for (var i = 0; i < INotebookStore.MaxNotes; i++) await alice.SaveAsync(new NoteDraft { Title = $"Note {i}" }, cancellationToken: cancellationToken);
        await Assert.ThrowsAsync<NotebookLimitException>(() => alice.SaveAsync(new NoteDraft { Title = "One too many" }, cancellationToken: cancellationToken));
        Assert.Single(await bob.ListAsync(cancellationToken));
    }

    private static (string Value, string Header) WorkspaceCookie(HttpResponseMessage response)
    {
        var header = response.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("learning.workspace=", StringComparison.Ordinal));
        return (Uri.UnescapeDataString(header.Split(';')[0]["learning.workspace=".Length..]), header);
    }

    private static async Task<string> Token(HttpClient client, string route)
    {
        var document = new HtmlParser().ParseDocument(await client.GetStringAsync(route, TestContext.Current.CancellationToken));
        return document.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;
    }

    private static async Task<HttpResponseMessage> Post(HttpClient client, string route, string token, string? name = null, string? value = null)
    {
        var form = new Dictionary<string, string> { ["__RequestVerificationToken"] = token };
        if (name is not null) form[name] = value!;
        using var content = new FormUrlEncodedContent(form);
        return await client.PostAsync(route, content, TestContext.Current.CancellationToken);
    }
}

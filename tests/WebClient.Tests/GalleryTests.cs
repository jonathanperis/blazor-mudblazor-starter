using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Utilities;
using WebClient.Shared.Components.Gallery.Actions;
using WebClient.Shared.Features.Gallery;
using WebClient.Shared.Features.Learning;

namespace WebClient.Tests;

/// <summary>Every gallery page and page sample renders, and every example can show its own source.</summary>
public sealed class GalleryTests
{
    public static TheoryData<string> Pages()
    {
        var data = new TheoryData<string>();
        foreach (var entry in GalleryCatalog.Components) data.Add(entry.Route);
        foreach (var sample in GalleryCatalog.Samples) data.Add(sample.Route);
        return data;
    }

    [Theory]
    [MemberData(nameof(Pages))]
    public async Task Page_renders_every_example_without_an_error_boundary(string route)
    {
        await using var context = new BunitContext();
        context.Services.AddLearningLabs(ComponentTests.ServerHost);
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Render<MudPopoverProvider>();
        context.Render<MudDialogProvider>();
        context.Render<MudSnackbarProvider>();
        var page = GalleryCatalog.Components.Select(entry => (entry.Route, entry.Page)).Concat(GalleryCatalog.Samples.Select(sample => (sample.Route, sample.Page))).Single(item => item.Route == route).Page;
        var rendered = context.Render(builder => { builder.OpenComponent(0, page); builder.CloseComponent(); });
        rendered.WaitForAssertion(() => Assert.NotEmpty(rendered.FindAll("h1")));
        Assert.DoesNotContain("failed to render", rendered.Markup);
        Assert.Single(rendered.FindAll("h1"));
    }

    [Fact]
    public void Catalog_routes_are_unique_and_every_example_embeds_its_source()
    {
        Assert.NotEmpty(GalleryCatalog.Components);
        var routes = GalleryCatalog.Components.Select(entry => entry.Route).Concat(GalleryCatalog.Samples.Select(sample => sample.Route)).ToList();
        Assert.Equal(routes.Count, routes.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(GalleryCatalog.Components, entry => Assert.StartsWith("/components/", entry.Route));
        Assert.All(GalleryCatalog.Samples, sample => Assert.StartsWith("/samples/", sample.Route));
        var examples = typeof(GalleryCatalog).Assembly.GetExportedTypes()
            .Where(type => type.Namespace?.StartsWith("WebClient.Shared.Components.Gallery.", StringComparison.Ordinal) == true)
            .Where(type => typeof(IComponent).IsAssignableFrom(type) && type.GetCustomAttribute<RouteAttribute>() is null);
        Assert.All(examples, example => Assert.False(string.IsNullOrWhiteSpace(ExampleSources.Get(example)), $"{example.FullName} has no embedded source"));
    }

    [Fact]
    public void Highlighter_classifies_razor_and_round_trips_the_source()
    {
        const string source = "@* note *@\n<MudButton Variant=\"Variant.Filled\" OnClick=\"Save\">Save</MudButton>\n@code {\n    private int _count = 42; // answer\n}";
        var tokens = SourceHighlighter.Tokenize(source);
        Assert.Equal(source, string.Concat(tokens.Select(token => token.Text)));
        Assert.Contains(tokens, token => token is { Kind: TokenKind.Comment, Text: "@* note *@" });
        Assert.Contains(tokens, token => token is { Kind: TokenKind.Tag, Text: "<MudButton" });
        Assert.Contains(tokens, token => token is { Kind: TokenKind.Attribute, Text: "Variant" });
        Assert.Contains(tokens, token => token is { Kind: TokenKind.String, Text: "\"Variant.Filled\"" });
        Assert.Contains(tokens, token => token is { Kind: TokenKind.Razor, Text: "@code" });
        Assert.Contains(tokens, token => token is { Kind: TokenKind.Keyword, Text: "private" });
        Assert.Contains(tokens, token => token is { Kind: TokenKind.Number, Text: "42" });
        Assert.DoesNotContain(tokens, token => token.Kind == TokenKind.Type && token.Text == "Save");
    }

    [Fact]
    public void Snippet_omits_defaults_and_wraps_long_attribute_lists()
    {
        Assert.Equal("<MudButton>Go</MudButton>", Snippet.Element("MudButton", [("Variant", Snippet.Enum(Variant.Text, Variant.Text)), ("Disabled", Snippet.Flag(false))], "Go"));
        Assert.Equal("<MudButton Variant=\"Variant.Filled\">Go</MudButton>", Snippet.Element("MudButton", [("Variant", Snippet.Enum(Variant.Filled, Variant.Text))], "Go"));
        var wrapped = Snippet.Element("MudTextField", [("Label", "A rather long label for wrapping"), ("Variant", "Variant.Outlined"), ("Margin", "Margin.Dense"), ("Clearable", "true")]);
        Assert.Contains("\n    Variant=\"Variant.Outlined\"", wrapped);
        Assert.EndsWith(" />", wrapped);
    }

    [Fact]
    public void Quick_search_indexes_every_destination_and_ranks_titles_first()
    {
        var all = SiteSearch.All;
        Assert.Equal(LabCatalog.All.Count, all.Count(entry => entry.Kind == SearchKind.Lab));
        Assert.Equal(GalleryCatalog.Components.Count, all.Count(entry => entry.Kind == SearchKind.Component));
        Assert.Equal(GalleryCatalog.Samples.Count, all.Count(entry => entry.Kind == SearchKind.Sample));
        Assert.True(all.Count(entry => entry.Kind == SearchKind.Example) > 400);
        // Every example anchor names a real example component, so the link lands on it.
        var exampleTypes = typeof(GalleryCatalog).Assembly.GetExportedTypes().Select(type => type.Name.ToLowerInvariant()).ToHashSet();
        Assert.All(all.Where(entry => entry.Kind == SearchKind.Example), entry => Assert.Contains(entry.Href.Split('#')[1], exampleTypes));
        Assert.All(all, entry => Assert.False(entry.Href.StartsWith('/'), $"{entry.Href} must be base-relative"));

        Assert.Equal("components/data-grid", SiteSearch.Find("data grid")[0].Href);
        Assert.Contains(SiteSearch.Find("playground alert"), entry => entry.Href == "components/alert#alertplayground");
        Assert.All(SiteSearch.Find(""), entry => Assert.Equal(SearchKind.Lab, entry.Kind));
        Assert.Empty(SiteSearch.Find("no such component anywhere"));
    }

    [Fact]
    public void Playground_state_parses_only_bounded_known_values()
    {
        Assert.Equal(new Dictionary<string, string> { ["playground"] = "x", ["label"] = "Two words", ["empty"] = "" },
            PlaygroundState.Query("http://host/page?playground=x&label=Two%20words&empty=#x"));
        Assert.True(PlaygroundState.TryParse(typeof(Variant), "Outlined", out var variant));
        Assert.Equal(Variant.Outlined, variant);
        Assert.False(PlaygroundState.TryParse(typeof(Variant), "1", out _));
        Assert.False(PlaygroundState.TryParse(typeof(Variant), "Text,Filled", out _));
        Assert.False(PlaygroundState.TryParse(typeof(int), "1000000", out _));
        Assert.False(PlaygroundState.TryParse(typeof(int), "-1", out _));
        Assert.True(PlaygroundState.TryParse(typeof(int?), "", out var none));
        Assert.Null(none);
        Assert.True(PlaygroundState.TryParse(typeof(decimal), "2.5", out var number));
        Assert.Equal(2.5m, number);
        Assert.False(PlaygroundState.TryParse(typeof(string), new string('x', PlaygroundState.MaxTextLength + 1), out _));
        Assert.True(PlaygroundState.TryParse(typeof(MudColor), "#1F5F5BFF", out var color));
        Assert.Equal("#1f5f5bff", PlaygroundState.Format(color).ToLowerInvariant());
        Assert.False(PlaygroundState.TryParse(typeof(MudColor), "rgb(0,0,0)", out _));
    }

    [Fact]
    public async Task Playground_restores_settings_from_a_link_and_resets_them()
    {
        await using var context = new BunitContext();
        context.Services.AddLearningLabs(ComponentTests.ServerHost);
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Render<MudPopoverProvider>();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("components/button?playground=buttonplayground&label=Shared%20link&disabled=true&variant=Nope");
        var playground = context.Render<ButtonPlayground>();

        playground.WaitForAssertion(() => Assert.Contains("Disabled=\"true\"", playground.Find(".playground-code").TextContent));
        Assert.Contains("Shared link", playground.Find(".playground-stage").TextContent);
        Assert.Contains("Variant.Filled", playground.Find(".playground-code").TextContent);
        Assert.NotNull(playground.Find(".playground-restored"));

        await playground.InvokeAsync(() => playground.FindAll("button").Single(button => button.TextContent.Contains("Reset")).Click());
        Assert.DoesNotContain("Disabled", playground.Find(".playground-code").TextContent);
        Assert.Contains("Buy tickets", playground.Find(".playground-stage").TextContent);
    }
}

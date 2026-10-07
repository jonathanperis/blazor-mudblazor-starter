using System.Reflection;
using Bunit;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
}

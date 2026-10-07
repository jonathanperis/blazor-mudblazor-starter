using System.Reflection;
using Microsoft.AspNetCore.Components;
using WebClient.Shared.Features.Learning;

namespace WebClient.Shared.Features.Gallery;

public sealed record GalleryEntry(string Title, string Route, GalleryGroup Group, string Summary, string? MudDocs, string[] Types, Type Page)
{
    public string Href => Route.TrimStart('/');
    public string Slug => Route[(Route.LastIndexOf('/') + 1)..];
    public string? MudDocsUrl => MudDocs is null ? null : $"https://mudblazor.com/components/{MudDocs}";
    public string SourceUrl => GalleryCatalog.SourceUrl(Page);
}

public sealed record SampleEntry(string Title, string Route, string Summary, string[] Uses, Type Page)
{
    public string Href => Route.TrimStart('/');
    public string SourceUrl => GalleryCatalog.SourceUrl(Page);
}

/// <summary>Gallery pages and page samples, discovered from their attributes and routes.</summary>
public static class GalleryCatalog
{
    private static readonly Type[] PageTypes = typeof(GalleryCatalog).Assembly.GetExportedTypes();

    public static IReadOnlyList<GalleryEntry> Components { get; } = PageTypes
        .Select(type => (type, meta: type.GetCustomAttribute<ComponentPageAttribute>(), route: Route(type)))
        .Where(page => page.meta is not null && page.route is not null)
        .Select(page => new GalleryEntry(page.meta!.Title, page.route!, page.meta.Group, page.meta.Summary, page.meta.MudDocs, page.meta.Types, page.type))
        .OrderBy(entry => entry.Group).ThenBy(entry => entry.Title, StringComparer.Ordinal)
        .ToList();

    public static IReadOnlyList<SampleEntry> Samples { get; } = PageTypes
        .Select(type => (type, meta: type.GetCustomAttribute<PageSampleAttribute>(), route: Route(type)))
        .Where(page => page.meta is not null && page.route is not null)
        .Select(page => new SampleEntry(page.meta!.Title, page.route!, page.meta.Summary, page.meta.Uses, page.type))
        .OrderBy(entry => entry.Title, StringComparer.Ordinal)
        .ToList();

    public static string GroupTitle(GalleryGroup group) => group switch
    {
        GalleryGroup.Actions => "Actions",
        GalleryGroup.Inputs => "Inputs and forms",
        GalleryGroup.Pickers => "Pickers and uploads",
        GalleryGroup.DataDisplay => "Data display",
        GalleryGroup.Charts => "Charts",
        GalleryGroup.Feedback => "Feedback and overlays",
        GalleryGroup.Navigation => "Navigation",
        GalleryGroup.Layout => "Layout and structure",
        _ => group.ToString()
    };

    public static GalleryEntry? ForPage(Type page) => Components.FirstOrDefault(entry => entry.Page == page);

    /// <summary>Repository path of a component, derived from its namespace (folders mirror namespaces).</summary>
    public static string SourcePath(Type component)
    {
        const string root = "WebClient.Shared.";
        var ns = component.Namespace ?? "";
        var relative = ns.StartsWith(root, StringComparison.Ordinal) ? ns[root.Length..].Replace('.', '/') : ns.Replace('.', '/');
        return $"src/WebClient.Shared/{relative}/{component.Name}.razor";
    }

    public static string SourceUrl(Type component) => $"{LabCatalog.Repository}/blob/main/{SourcePath(component)}";

    private static string? Route(Type type) => type.GetCustomAttributes<RouteAttribute>().Select(route => route.Template).FirstOrDefault();
}

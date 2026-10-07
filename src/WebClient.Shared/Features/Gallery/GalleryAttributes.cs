namespace WebClient.Shared.Features.Gallery;

/// <summary>Families in the component gallery, in reading order.</summary>
public enum GalleryGroup
{
    Actions,
    Inputs,
    Pickers,
    DataDisplay,
    Charts,
    Feedback,
    Navigation,
    Layout,
    Theming
}

/// <summary>
/// Marks a routable page as a component gallery entry. The catalog discovers pages by this attribute, so adding a
/// component never requires editing a shared list. The route comes from the page's <c>@page</c> directive.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ComponentPageAttribute(string title, GalleryGroup group, string summary) : Attribute
{
    public string Title { get; } = title;
    public GalleryGroup Group { get; } = group;
    public string Summary { get; } = summary;
    /// <summary>Path on mudblazor.com after <c>/components/</c>, for example "button".</summary>
    public string? MudDocs { get; init; }
    /// <summary>MudBlazor types demonstrated on the page, for search.</summary>
    public string[] Types { get; init; } = [];
}

/// <summary>Marks a routable page as a complete page sample composed from many components.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PageSampleAttribute(string title, string summary) : Attribute
{
    public string Title { get; } = title;
    public string Summary { get; } = summary;
    /// <summary>Components the sample combines, shown on the samples index.</summary>
    public string[] Uses { get; init; } = [];
}

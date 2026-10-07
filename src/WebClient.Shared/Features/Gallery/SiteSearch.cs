using System.Text.RegularExpressions;
using WebClient.Shared.Features.Learning;

namespace WebClient.Shared.Features.Gallery;

public enum SearchKind { Lab, Component, Sample, Example }

/// <summary>One destination in quick search. <see cref="Href"/> is base-relative, like every link in the shell.</summary>
public sealed record SearchEntry(string Title, string Href, SearchKind Kind, string Context, string Keywords)
{
    public string KindLabel => Kind switch
    {
        SearchKind.Lab => "Lab",
        SearchKind.Component => "Component",
        SearchKind.Sample => "Page sample",
        _ => "Example"
    };
}

/// <summary>
/// The quick search index: labs, gallery pages, page samples and every example on a gallery page. Examples come from
/// the embedded page sources, so an example is searchable as soon as a page lists it.
/// </summary>
public static class SiteSearch
{
    private static readonly Regex ExampleTag = new(@"<Example\s(?<attributes>(?:[^""/]|""[^""]*"")*)/>", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly Regex Attribute = new(@"(?<name>Of|Title|Description)=""(?:typeof\((?<type>\w+)\)|(?<value>[^""]*))""", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
    private static readonly Lazy<IReadOnlyList<SearchEntry>> Index = new(Build);

    public static IReadOnlyList<SearchEntry> All => Index.Value;

    /// <summary>Entries containing every word of the query, best match first. An empty query lists the labs.</summary>
    public static IReadOnlyList<SearchEntry> Find(string? query, int limit = 12)
    {
        var terms = (query ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0) return All.Where(entry => entry.Kind == SearchKind.Lab).Take(limit).ToList();
        var phrase = string.Join(' ', terms);
        return All
            .Where(entry => terms.All(term => Contains(entry.Title, term) || Contains(entry.Context, term) || Contains(entry.Keywords, term)))
            .OrderBy(entry => Rank(entry, phrase))
            .ThenBy(entry => entry.Kind)
            .ThenBy(entry => entry.Title, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
    }

    // Title matches first: whole title, then its start, then anywhere; then matches only in context or keywords.
    private static int Rank(SearchEntry entry, string phrase) =>
        entry.Title.Equals(phrase, StringComparison.OrdinalIgnoreCase) ? 0
        : entry.Title.StartsWith(phrase, StringComparison.OrdinalIgnoreCase) ? 1
        : Contains(entry.Title, phrase) ? 2
        : Contains(entry.Context, phrase) ? 3
        : 4;

    private static bool Contains(string text, string term) => text.Contains(term, StringComparison.OrdinalIgnoreCase);

    private static List<SearchEntry> Build()
    {
        var entries = new List<SearchEntry>();
        entries.AddRange(LabCatalog.All.Select(lab => new SearchEntry(lab.Title, lab.Href, SearchKind.Lab, $"{lab.Level} lab", lab.Objective)));
        foreach (var component in GalleryCatalog.Components)
        {
            entries.Add(new SearchEntry(component.Title, component.Href, SearchKind.Component, GalleryCatalog.GroupTitle(component.Group),
                $"{string.Join(' ', component.Types)} {component.Summary}"));
            entries.AddRange(Examples(component).Select(example => new SearchEntry(example.Title, $"{component.Href}#{example.Anchor}",
                SearchKind.Example, component.Title, $"{string.Join(' ', component.Types)} {example.Description}")));
        }
        entries.AddRange(GalleryCatalog.Samples.Select(sample => new SearchEntry(sample.Title, sample.Href, SearchKind.Sample, "Page sample",
            $"{string.Join(' ', sample.Uses)} {sample.Summary}")));
        return entries;
    }

    /// <summary>The examples a gallery page lists, read from its embedded source. Anchors match <c>Example</c>'s.</summary>
    public static IEnumerable<(string Title, string Anchor, string Description)> Examples(GalleryEntry component)
    {
        var source = ExampleSources.Get(component.Page);
        if (source is null) yield break;
        foreach (Match tag in ExampleTag.Matches(source))
        {
            string? type = null, title = null, description = null;
            foreach (Match attribute in Attribute.Matches(tag.Groups["attributes"].Value))
            {
                switch (attribute.Groups["name"].Value)
                {
                    case "Of": type = attribute.Groups["type"].Value; break;
                    case "Title": title = attribute.Groups["value"].Value; break;
                    default: description = attribute.Groups["value"].Value; break;
                }
            }
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(title))
                yield return (title, type.ToLowerInvariant(), description ?? "");
        }
    }
}

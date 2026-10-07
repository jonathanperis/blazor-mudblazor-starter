using System.Collections.Concurrent;

namespace WebClient.Shared.Features.Gallery;

/// <summary>
/// The Razor source of each gallery example, embedded in the assembly at build time. Showing the embedded file
/// guarantees that the code on screen is exactly the code that rendered the preview.
/// </summary>
public static class ExampleSources
{
    private static readonly ConcurrentDictionary<Type, string?> Cache = new();

    public static string? Get(Type example) => Cache.GetOrAdd(example, Load);

    private static string? Load(Type example)
    {
        var path = GalleryCatalog.SourcePath(example)["src/WebClient.Shared/".Length..];
        var assembly = typeof(ExampleSources).Assembly;
        // LogicalName uses the build machine's directory separator.
        using var stream = assembly.GetManifestResourceStream(path) ?? assembly.GetManifestResourceStream(path.Replace('/', '\\'));
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }
}

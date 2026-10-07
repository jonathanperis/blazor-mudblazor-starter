namespace WebClient.Shared.Features.Gallery;

/// <summary>
/// Builds the markup a playground displays. Attributes whose value is null are omitted, so the snippet shows only
/// what differs from the component's defaults.
/// </summary>
public static class Snippet
{
    public static string Element(string tag, IEnumerable<(string Name, string? Value)> attributes, string? content = null)
    {
        var present = attributes.Where(attribute => attribute.Value is not null).Select(attribute => attribute.Value == "" ? attribute.Name : $"{attribute.Name}=\"{attribute.Value}\"").ToList();
        var opening = present.Count switch
        {
            0 => $"<{tag}",
            <= 3 when present.Sum(text => text.Length) < 70 => $"<{tag} {string.Join(' ', present)}",
            _ => $"<{tag}\n    {string.Join("\n    ", present)}"
        };
        return content is null ? $"{opening} />" : $"{opening}>{content}</{tag}>";
    }

    /// <summary>The attribute value for an enum parameter, or null when it equals the default.</summary>
    public static string? Enum<T>(T value, T defaultValue) where T : struct, System.Enum =>
        EqualityComparer<T>.Default.Equals(value, defaultValue) ? null : $"{typeof(T).Name}.{value}";

    /// <summary>A boolean attribute: present when true, omitted when false.</summary>
    public static string? Flag(bool value) => value ? "true" : null;
}

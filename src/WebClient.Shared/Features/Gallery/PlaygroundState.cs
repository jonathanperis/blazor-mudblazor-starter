using System.Globalization;
using System.Reflection;
using MudBlazor.Utilities;

namespace WebClient.Shared.Features.Gallery;

/// <summary>
/// Reads and writes a playground's settings as text, so a link can carry them. A setting is any private,
/// non-readonly field of the playground whose name starts with an underscore and whose type is text, a number,
/// a switch, an enum or a color. Collections, references and derived properties are never part of a link.
/// </summary>
public static class PlaygroundState
{
    /// <summary>Longest text value accepted from a link.</summary>
    public const int MaxTextLength = 200;
    /// <summary>Numbers from a link must fall in this range, so a crafted link cannot ask for a million rows.</summary>
    public const int MaxNumber = 1000;

    public static IReadOnlyList<FieldInfo> Fields(Type owner) => owner
        .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
        .Where(field => field.Name.StartsWith('_') && !field.IsInitOnly && IsSupported(field.FieldType))
        .ToList();

    /// <summary>Query-string name of a field: <c>_startIcon</c> becomes <c>startIcon</c>.</summary>
    public static string Key(FieldInfo field) => field.Name.TrimStart('_');

    /// <summary>The current value of every setting, keyed by <see cref="Key"/>.</summary>
    public static Dictionary<string, string> Capture(object owner) =>
        Fields(owner.GetType()).ToDictionary(Key, field => Format(field.GetValue(owner)), StringComparer.Ordinal);

    /// <summary>Applies the values that parse and are in range; ignores unknown keys. Returns how many were applied.</summary>
    public static int Apply(object owner, IReadOnlyDictionary<string, string> values)
    {
        var applied = 0;
        foreach (var field in Fields(owner.GetType()))
        {
            if (!values.TryGetValue(Key(field), out var text) || !TryParse(field.FieldType, text, out var value)) continue;
            field.SetValue(owner, value);
            applied++;
        }
        return applied;
    }

    public static string Format(object? value) => value switch
    {
        null => "",
        bool flag => flag ? "true" : "false",
        MudColor color => color.ToString(MudColorOutputFormats.HexA),
        IFormattable number when value is not Enum => number.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };

    public static bool TryParse(Type type, string text, out object? value)
    {
        value = null;
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null && text.Length == 0) return true;
        var target = underlying ?? type;
        if (target == typeof(string))
        {
            value = text;
            return text.Length <= MaxTextLength;
        }
        if (target == typeof(bool))
        {
            if (!bool.TryParse(text, out var flag)) return false;
            value = flag;
            return true;
        }
        if (target.IsEnum)
        {
            // Names only: Enum.TryParse would also accept numbers and comma lists that name no member.
            if (!Enum.GetNames(target).Contains(text, StringComparer.Ordinal)) return false;
            value = Enum.Parse(target, text);
            return true;
        }
        if (target == typeof(MudColor))
        {
            if (text.Length is not (7 or 9) || text[0] != '#' || !text[1..].All(char.IsAsciiHexDigit)) return false;
            value = new MudColor(text);
            return true;
        }
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number) || number is < 0 or > MaxNumber) return false;
        value = Convert.ChangeType(number, target, CultureInfo.InvariantCulture);
        return true;
    }

    /// <summary>Query parameters of an absolute or relative address. Later duplicates win.</summary>
    public static Dictionary<string, string> Query(string uri)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var start = uri.IndexOf('?');
        if (start < 0) return result;
        var end = uri.IndexOf('#', start);
        var query = end < 0 ? uri[(start + 1)..] : uri[(start + 1)..end];
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            var name = Decode(separator < 0 ? pair : pair[..separator]);
            result[name] = separator < 0 ? "" : Decode(pair[(separator + 1)..]);
        }
        return result;
    }

    private static string Decode(string text) => Uri.UnescapeDataString(text.Replace('+', ' '));

    private static bool IsSupported(Type type)
    {
        var target = Nullable.GetUnderlyingType(type) ?? type;
        return target == typeof(string) || target == typeof(bool) || target.IsEnum || target == typeof(MudColor)
            || target == typeof(int) || target == typeof(long) || target == typeof(double) || target == typeof(float) || target == typeof(decimal);
    }
}

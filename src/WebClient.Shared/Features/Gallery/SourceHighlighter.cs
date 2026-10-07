using System.Text.RegularExpressions;

namespace WebClient.Shared.Features.Gallery;

public enum TokenKind { Text, Comment, String, Tag, Attribute, Razor, Keyword, Type, Number }

public readonly record struct SourceToken(TokenKind Kind, string Text);

/// <summary>
/// A small Razor and C# tokenizer for the gallery's code view. It only classifies text; rendering stays plain
/// text, so source code is never interpreted as markup.
/// </summary>
public static partial class SourceHighlighter
{
    [GeneratedRegex("""
        (?<comment>@\*[\s\S]*?\*@|<!--[\s\S]*?-->|//[^\n]*|/\*[\s\S]*?\*/)
        |(?<string>@?"(?:[^"\\\n]|\\.)*"|'(?:\\.|[^'\\\n])')
        |(?<tag></?[A-Za-z][\w.:-]*|/?>)
        |(?<razor>@(?:page|code|inject|using|attribute|implements|inherits|layout|typeparam|rendermode|foreach|for|if|else|switch|while|bind(?:-[\w:]+)?|on\w+(?::\w+)?|key|ref)\b|@(?=[\w(]))
        |(?<attribute>\b[A-Za-z_][\w.-]*(?==))
        |(?<keyword>\b(?:private|public|protected|internal|static|readonly|const|var|new|return|if|else|foreach|for|in|while|switch|case|default|break|async|await|void|int|bool|string|double|decimal|float|long|object|true|false|null|class|record|struct|enum|this|typeof|nameof|get|set|init|override|sealed|is|not|and|or|when|with)\b)
        |(?<type>\b[A-Z][A-Za-z0-9]*(?=\s*[.(\[]|<(?!/)|\s+[a-z_]\w*\s*[=;,)]))
        |(?<number>\b\d+(?:\.\d+)?\b)
        """, RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex Tokens();

    private static readonly (string Group, TokenKind Kind)[] Groups =
    [
        ("comment", TokenKind.Comment), ("string", TokenKind.String), ("tag", TokenKind.Tag), ("razor", TokenKind.Razor),
        ("attribute", TokenKind.Attribute), ("keyword", TokenKind.Keyword), ("type", TokenKind.Type), ("number", TokenKind.Number)
    ];

    public static IReadOnlyList<SourceToken> Tokenize(string source)
    {
        var tokens = new List<SourceToken>();
        var position = 0;
        foreach (Match match in Tokens().Matches(source))
        {
            if (match.Index > position) tokens.Add(new(TokenKind.Text, source[position..match.Index]));
            var kind = Groups.First(group => match.Groups[group.Group].Success).Kind;
            tokens.Add(new(kind, match.Value));
            position = match.Index + match.Length;
        }
        if (position < source.Length) tokens.Add(new(TokenKind.Text, source[position..]));
        return tokens;
    }
}

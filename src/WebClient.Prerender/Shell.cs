using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace WebClient.Prerender;

/// <summary>Places a prerendered page into the published index.html.</summary>
internal static class Shell
{
    public static string Compose(string shell, string url, string body, string head, string description, string dark)
    {
        var title = Regex.Match(head, "<title>(.*?)</title>", RegexOptions.Singleline) is { Success: true } match ? match.Groups[1].Value : "Blazor learning sandbox";
        var attribute = HtmlEncoder.Default;
        var meta = $"""
            <title>{title}</title>
                <meta name="description" content="{attribute.Encode(description)}" />
                <link rel="canonical" href="{attribute.Encode(url)}" />
                <meta property="og:type" content="website" />
                <meta property="og:site_name" content="Blazor learning sandbox" />
                <meta property="og:title" content="{title}" />
                <meta property="og:description" content="{attribute.Encode(description)}" />
                <meta property="og:url" content="{attribute.Encode(url)}" />
                <meta name="twitter:card" content="summary" />
            """;
        var html = Replace(shell, @"<title>.*?</title>\s*<meta name=""description"" content=""[^""]*"" />", meta, "title and description");
        // The page as rendered in light mode, a note while .NET starts, and the dark palette for visitors who chose it.
        var app = $"""
            <div id="app">{body}
                <p class="demo-booting" role="status">Starting .NET in your browser…</p>
                <template id="prerendered-dark-theme">{dark}</template>
                <script>{ThemeScript}</script>
                </div>
            """;
        return Replace(html, @"<div id=""app"">.*?</div>\s*(?=<div id=""blazor-error-ui"")", app + "\n    ", "#app");
    }

    // The theme mirrors learningPreferences.read in learning.js, which loads after this content: a stored choice, else
    // the system's. The dark palette's variables follow the light ones, so they win; the layout class switches the code colors.
    // Pages are prerendered in English; a visitor who chose another culture sees only the start-up note until the app
    // renders in that culture (learningCulture in learning.js reads the same key). Everything this script adds lives
    // inside #app, so Blazor removes it with the prerendered page on its first render.
    private const string ThemeScript = """(function () { try { var app = document.getElementById("app"); var culture = localStorage.getItem("culture"); if (culture && culture !== "en-US") app.insertAdjacentHTML("beforeend", "<style>#app > :not(.demo-booting) { visibility: hidden; }</style>"); var stored = localStorage.getItem("isDarkMode"); var dark = stored === null ? matchMedia("(prefers-color-scheme: dark)").matches : stored.toLowerCase() === "true"; if (!dark) return; app.appendChild(document.getElementById("prerendered-dark-theme").content.cloneNode(true)); var layout = app.querySelector(".sandbox-light"); if (layout) layout.classList.replace("sandbox-light", "sandbox-dark"); } catch (e) { } })();""";

    private static string Replace(string html, string pattern, string replacement, string what)
    {
        var regex = new Regex(pattern, RegexOptions.Singleline);
        if (regex.Matches(html).Count != 1) throw new InvalidOperationException($"The published index.html must contain exactly one {what}.");
        return regex.Replace(html, replacement.Replace("$", "$$"), 1);
    }
}

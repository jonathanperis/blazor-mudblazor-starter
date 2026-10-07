using System.Globalization;
using System.Net;
using System.Text;

namespace WebClient.Shared.Components.Samples.Storefront;

/// <summary>A book on the shelves of Marginalia, a fictional independent bookshop. Covers are drawn, not downloaded.</summary>
public sealed record Book(
    int Id, string Title, string Author, string Category, decimal Price, double Rating, int Reviews,
    bool InStock, string? Badge, BookCover Cover, int Featured);

/// <summary>Paper, ink and a motif: enough to draw a cover as inline SVG.</summary>
public sealed record BookCover(string Paper, string Ink, CoverMotif Motif);

public enum CoverMotif { Sun, Rings, Stripes, Waves, Grid, Arch, Leaf, Stars }

public enum BookSort { Featured, PriceLowToHigh, PriceHighToLow, TopRated, TitleAToZ }

public sealed class BasketLine(Book book)
{
    public const int MaxQuantity = 10;
    public Book Book { get; } = book;
    public int Quantity { get; set; } = 1;
    public decimal Total => Book.Price * Quantity;
}

public static class StorefrontData
{
    public const decimal FreeShippingThreshold = 45m;
    public const decimal ShippingFee = 4.5m;
    public const decimal MaxPrice = 40m;

    public static readonly string[] Categories = ["Fiction", "Mystery", "Science", "History", "Poetry", "Cookery", "Travel"];

    /// <summary>The shop prices in pounds whatever the visitor's culture, so the format is fixed.</summary>
    public static string Money(decimal amount) => "£" + amount.ToString("0.00", CultureInfo.InvariantCulture);

    public static string SortLabel(BookSort sort) => sort switch
    {
        BookSort.PriceLowToHigh => "Price: low to high",
        BookSort.PriceHighToLow => "Price: high to low",
        BookSort.TopRated => "Highest rated",
        BookSort.TitleAToZ => "Title: A to Z",
        _ => "Staff favourites"
    };

    private static readonly BookCover Vermilion = new("#B4441F", "#FBEFE6", CoverMotif.Sun);
    private static readonly BookCover Teal = new("#1F5F5B", "#E4F2EF", CoverMotif.Waves);
    private static readonly BookCover Ochre = new("#C99A3B", "#2A2116", CoverMotif.Grid);
    private static readonly BookCover Ink = new("#2A2824", "#EDE6D6", CoverMotif.Stars);
    private static readonly BookCover Slate = new("#2B5C8A", "#E6EEF6", CoverMotif.Rings);
    private static readonly BookCover Moss = new("#3F6B3A", "#EEF3E4", CoverMotif.Leaf);
    private static readonly BookCover Plum = new("#5B3A5E", "#F3E8F0", CoverMotif.Arch);
    private static readonly BookCover Paper = new("#E9DFC9", "#3B2F22", CoverMotif.Stripes);

    public static IReadOnlyList<Book> Books { get; } =
    [
        new(1, "The Lighthouse Keeper's Almanac", "Ines Varga", "Fiction", 18.50m, 4.6, 214, true, "Staff pick", Slate with { Motif = CoverMotif.Rings }, 1),
        new(2, "A Quiet Murder in Harrow Lane", "Desmond Achterberg", "Mystery", 14.99m, 4.2, 388, true, null, Ink with { Motif = CoverMotif.Arch }, 6),
        new(3, "Salt, Smoke and Citrus", "Pilar Okonjo", "Cookery", 32.00m, 4.8, 96, true, "Signed copy", Vermilion, 2),
        new(4, "The Shape of Small Things", "Dr. Hana Lindqvist", "Science", 22.00m, 4.5, 143, true, null, Teal with { Motif = CoverMotif.Grid }, 8),
        new(5, "Rivers Without Maps", "Tomás Ferreira", "Travel", 19.75m, 4.1, 61, false, null, Moss with { Motif = CoverMotif.Waves }, 15),
        new(6, "Ledgers of the Wool Road", "Margit Sallow", "History", 27.50m, 4.4, 77, true, "New", Ochre, 4),
        new(7, "Field Notes for Late Swallows", "Oluwaseun Abara", "Poetry", 12.00m, 4.9, 52, true, "Staff pick", Paper with { Motif = CoverMotif.Leaf }, 3),
        new(8, "The Clockmaker's Alibi", "Rosalind Petch", "Mystery", 13.50m, 3.9, 412, true, null, Plum, 12),
        new(9, "Bread at the Edge of Winter", "Agnes Thornbury", "Cookery", 26.00m, 4.3, 128, false, null, Paper with { Motif = CoverMotif.Sun }, 11),
        new(10, "What the Tide Remembers", "Kenji Morrow", "Fiction", 16.99m, 4.0, 302, true, null, Teal, 9),
        new(11, "Under a Borrowed Sky", "Céleste Amari", "Travel", 21.00m, 4.7, 88, true, "New", Slate with { Motif = CoverMotif.Sun }, 5),
        new(12, "Atlas of Vanished Islands", "Bram de Wit", "History", 36.00m, 4.6, 41, true, null, Moss with { Motif = CoverMotif.Rings }, 14),
        new(13, "Ten Thousand Kinds of Moss", "Dr. Ada Ferrand", "Science", 17.50m, 4.2, 67, true, null, Moss, 17),
        new(14, "The Glass Orchard", "Lena Brasch", "Fiction", 15.00m, 3.7, 156, true, null, Vermilion with { Motif = CoverMotif.Leaf }, 16),
        new(15, "Small Hours", "Faisal Rahman", "Poetry", 11.00m, 4.4, 39, false, "Signed copy", Ink with { Motif = CoverMotif.Sun }, 13),
        new(16, "The Cartographer's Daughter", "Ines Varga", "Fiction", 17.25m, 4.3, 190, true, null, Ochre with { Motif = CoverMotif.Waves }, 7),
        new(17, "Cold Case at Fennick Mill", "Desmond Achterberg", "Mystery", 14.25m, 4.1, 233, true, "New", Slate with { Motif = CoverMotif.Stripes }, 10),
        new(18, "How Comets Keep Time", "Dr. Hana Lindqvist", "Science", 24.00m, 4.8, 102, true, "Staff pick", Ink, 18),
        new(19, "Kitchens of the Old Port", "Pilar Okonjo", "Cookery", 29.50m, 4.0, 58, true, null, Teal with { Motif = CoverMotif.Arch }, 19),
        new(20, "The Night Ferry to Tallinn", "Céleste Amari", "Travel", 18.00m, 3.8, 74, true, null, Plum with { Motif = CoverMotif.Waves }, 20),
        new(21, "A Short History of Candlelight", "Margit Sallow", "History", 23.00m, 4.5, 63, false, null, Vermilion with { Motif = CoverMotif.Stars }, 21),
        new(22, "Weather for Beginners", "Oluwaseun Abara", "Poetry", 10.50m, 4.1, 27, true, null, Slate with { Motif = CoverMotif.Waves }, 22),
        new(23, "The Beekeeper's Ledger", "Rosalind Petch", "Mystery", 15.50m, 4.6, 145, true, null, Ochre with { Motif = CoverMotif.Arch }, 23),
        new(24, "Notes from a Night Train", "Tomás Ferreira", "Travel", 20.00m, 4.3, 49, true, null, Ink with { Motif = CoverMotif.Stripes }, 24)
    ];

    /// <summary>Draws a book cover as SVG markup. Text is HTML-encoded; nothing comes from outside the sample.</summary>
    public static string CoverSvg(Book book)
    {
        var (paper, ink) = (book.Cover.Paper, book.Cover.Ink);
        var svg = new StringBuilder();
        svg.Append($"""<svg viewBox="0 0 200 300" xmlns="http://www.w3.org/2000/svg" role="img" aria-label="Cover of {Encode(book.Title)}" style="display:block;width:100%;height:100%">""");
        svg.Append($"""<rect width="200" height="300" fill="{paper}"/><rect x="0" y="0" width="10" height="300" fill="{ink}" opacity=".14"/>""");
        svg.Append($"""<g fill="none" stroke="{ink}" stroke-width="1.5" opacity=".55">{Motif(book.Cover.Motif, ink)}</g>""");
        var lines = Wrap(book.Title, 15);
        for (var index = 0; index < lines.Count; index++)
        {
            svg.Append($"""<text x="24" y="{196 + index * 22 - (lines.Count - 1) * 11}" fill="{ink}" font-family="Fraunces, Georgia, serif" font-size="19" font-weight="500">{Encode(lines[index])}</text>""");
        }
        svg.Append($"""<line x1="24" y1="254" x2="60" y2="254" stroke="{ink}" stroke-width="1.5"/>""");
        svg.Append($"""<text x="24" y="274" fill="{ink}" font-family="IBM Plex Mono, monospace" font-size="9" letter-spacing="1.2" opacity=".85">{Encode(book.Author.ToUpperInvariant())}</text>""");
        svg.Append("</svg>");
        return svg.ToString();
    }

    private static string Motif(CoverMotif motif, string ink) => motif switch
    {
        CoverMotif.Sun => """<circle cx="120" cy="86" r="38"/><circle cx="120" cy="86" r="52" stroke-dasharray="2 6"/>""",
        CoverMotif.Rings => """<circle cx="100" cy="84" r="20"/><circle cx="100" cy="84" r="36"/><circle cx="100" cy="84" r="52"/><circle cx="100" cy="84" r="68"/>""",
        CoverMotif.Stripes => string.Concat(Enumerable.Range(0, 8).Select(i => $"""<line x1="24" y1="{36 + i * 14}" x2="176" y2="{36 + i * 14}"/>""")),
        CoverMotif.Waves => string.Concat(Enumerable.Range(0, 5).Select(i => $"""<path d="M24 {50 + i * 18} q 19 -12 38 0 t 38 0 t 38 0 t 38 0"/>""")),
        CoverMotif.Grid => string.Concat(Enumerable.Range(0, 5).Select(i => $"""<line x1="{40 + i * 30}" y1="30" x2="{40 + i * 30}" y2="140"/><line x1="40" y1="{30 + i * 27}" x2="160" y2="{30 + i * 27}"/>""")),
        CoverMotif.Arch => """<path d="M60 150 V90 a40 40 0 0 1 80 0 V150"/><path d="M76 150 V94 a24 24 0 0 1 48 0 V150"/><line x1="40" y1="150" x2="160" y2="150"/>""",
        CoverMotif.Leaf => $"""<path d="M100 150 C40 120 50 50 100 30 C150 50 160 120 100 150 Z"/><line x1="100" y1="40" x2="100" y2="150"/><circle cx="100" cy="30" r="3" fill="{ink}"/>""",
        _ => string.Concat(new[] { (52, 48), (130, 40), (96, 82), (150, 104), (64, 118), (118, 136) }.Select(p => $"""<path d="M{p.Item1} {p.Item2 - 7} V{p.Item2 + 7} M{p.Item1 - 7} {p.Item2} H{p.Item1 + 7}"/>"""))
    };

    private static List<string> Wrap(string text, int width)
    {
        var lines = new List<string>();
        var line = "";
        foreach (var word in text.Split(' '))
        {
            if (line.Length > 0 && line.Length + word.Length + 1 > width)
            {
                lines.Add(line);
                line = word;
            }
            else
            {
                line = line.Length == 0 ? word : $"{line} {word}";
            }
        }
        lines.Add(line);
        return lines;
    }

    private static string Encode(string text) => WebUtility.HtmlEncode(text);
}

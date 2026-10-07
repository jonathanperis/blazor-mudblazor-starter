using System.Globalization;
using System.Text;

namespace WebClient.Shared.Components.Samples.Admin;

public enum MemberStatus { Active, Suspended, Expired }

public enum MemberTier { Standard, Student, Senior, Family }

/// <summary>A library card holder. The grid shows copies; the edit dialog edits a <see cref="Copy"/>.</summary>
public sealed class LibraryMember
{
    public string CardNumber { get; init; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Branch { get; set; } = "";
    public MemberTier Tier { get; set; }
    public MemberStatus Status { get; set; }
    public DateTime Joined { get; init; }
    public int Loans { get; set; }
    public decimal Fines { get; set; }

    public string Initials => string.Concat(Name.Split(' ').Take(2).Select(part => part[0]));

    public LibraryMember Copy() => (LibraryMember)MemberwiseClone();
}

/// <summary>Selection survives reloads because members are compared by card number, not by instance.</summary>
public sealed class MemberComparer : IEqualityComparer<LibraryMember>
{
    public static readonly MemberComparer Instance = new();
    public bool Equals(LibraryMember? x, LibraryMember? y) => x?.CardNumber == y?.CardNumber;
    public int GetHashCode(LibraryMember member) => member.CardNumber.GetHashCode(StringComparison.Ordinal);
}

/// <summary>What the grid asks for: one page, one sort, and the toolbar filters.</summary>
public sealed record MemberQuery(int Page, int PageSize, string? SortBy, bool Descending, string? Search, string? Branch, MemberStatus? Status);

public sealed record MemberPage(IReadOnlyList<LibraryMember> Items, int Total);

/// <summary>
/// A pretend member service. Queries wait like a network call, honour cancellation, can fail on demand, and return
/// copies, so nothing on screen changes until a save goes through the service.
/// </summary>
public sealed class MemberDirectory
{
    public static readonly string[] Branches = ["Central", "Riverside", "Hilltop", "Old Mill"];
    public static readonly DateTime Today = new(2026, 10, 9);
    private readonly List<LibraryMember> _members = Generate();

    public TimeSpan Latency { get; set; } = TimeSpan.FromMilliseconds(450);
    public bool Unreliable { get; set; }

    public int Count => _members.Count;
    public int ActiveCount => _members.Count(member => member.Status == MemberStatus.Active);
    public int LoansOut => _members.Sum(member => member.Loans);
    public decimal FinesOwed => _members.Sum(member => member.Fines);
    public int JoinedThisMonth => _members.Count(member => member.Joined.Year == Today.Year && member.Joined.Month == Today.Month);

    public async Task<MemberPage> QueryAsync(MemberQuery query, CancellationToken token)
    {
        await Task.Delay(Latency, token);
        if (Unreliable) throw new HttpRequestException("The member service answered 503 Service Unavailable.");
        var matches = Sort(Filter(query), query.SortBy, query.Descending).ToList();
        var page = matches.Skip(query.Page * query.PageSize).Take(query.PageSize).Select(member => member.Copy()).ToList();
        return new MemberPage(page, matches.Count);
    }

    /// <summary>Every match for the filters, for export; same order as the grid.</summary>
    public IReadOnlyList<LibraryMember> Export(MemberQuery query) => Sort(Filter(query), query.SortBy, query.Descending).Select(member => member.Copy()).ToList();

    /// <summary>The given cards in the grid's order, ignoring the filters: a selection can span several searches.</summary>
    public IReadOnlyList<LibraryMember> Export(MemberQuery query, IEnumerable<string> cardNumbers)
    {
        var cards = cardNumbers.ToHashSet();
        return Sort(_members.Where(member => cards.Contains(member.CardNumber)), query.SortBy, query.Descending).Select(member => member.Copy()).ToList();
    }

    public void Save(LibraryMember changed)
    {
        var index = _members.FindIndex(member => member.CardNumber == changed.CardNumber);
        if (index >= 0) _members[index] = changed.Copy();
    }

    public void SetStatus(IEnumerable<LibraryMember> members, MemberStatus status)
    {
        var cards = members.Select(member => member.CardNumber).ToHashSet();
        foreach (var member in _members.Where(member => cards.Contains(member.CardNumber))) member.Status = status;
    }

    public void Delete(IEnumerable<LibraryMember> members)
    {
        var cards = members.Select(member => member.CardNumber).ToHashSet();
        _members.RemoveAll(member => cards.Contains(member.CardNumber));
    }

    private IEnumerable<LibraryMember> Filter(MemberQuery query) =>
        _members.Where(member => InFacets(member, query) && MatchesSearch(member, query.Search));

    private static bool InFacets(LibraryMember member, MemberQuery query) =>
        (query.Branch is null || member.Branch == query.Branch) && (query.Status is null || member.Status == query.Status);

    private static bool MatchesSearch(LibraryMember member, string? search) => string.IsNullOrWhiteSpace(search)
        || member.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
        || member.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
        || member.CardNumber.Contains(search, StringComparison.OrdinalIgnoreCase);

    // A real API would accept only known sort keys; unknown ones fall back to the card number.
    private static IEnumerable<LibraryMember> Sort(IEnumerable<LibraryMember> members, string? sortBy, bool descending)
    {
        Func<LibraryMember, object> key = sortBy switch
        {
            nameof(LibraryMember.Name) => member => member.Name,
            nameof(LibraryMember.Branch) => member => member.Branch,
            nameof(LibraryMember.Tier) => member => member.Tier,
            nameof(LibraryMember.Status) => member => member.Status,
            nameof(LibraryMember.Joined) => member => member.Joined,
            nameof(LibraryMember.Loans) => member => member.Loans,
            nameof(LibraryMember.Fines) => member => member.Fines,
            _ => member => member.CardNumber
        };
        return descending ? members.OrderByDescending(key) : members.OrderBy(key);
    }

    private static List<LibraryMember> Generate()
    {
        string[] first = ["Ada", "Bilal", "Carmen", "Dmitri", "Esther", "Farah", "Gideon", "Hana", "Idris", "Joan", "Kofi", "Leila", "Mateo", "Nell", "Oskar", "Priya", "Quentin", "Rosa", "Samir", "Tilde", "Umar", "Vera", "Wen", "Ximena", "Yusuf", "Zora"];
        string[] last = ["Abbott", "Baptiste", "Castellano", "Dlamini", "Eriksen", "Fairweather", "Gallo", "Hartmann", "Ibarra", "Jovanović", "Kowalczyk", "Lindgren", "Mbeki", "Novak", "Okafor", "Pereira", "Quinlan", "Rahman", "Sato", "Thorne", "Underwood", "Varga", "Whitlock", "Yilmaz"];
        var random = new Random(1987);
        var members = new List<LibraryMember>();
        for (var index = 0; index < 184; index++)
        {
            var name = $"{first[random.Next(first.Length)]} {last[random.Next(last.Length)]}";
            var status = random.Next(100) switch { < 80 => MemberStatus.Active, < 90 => MemberStatus.Expired, _ => MemberStatus.Suspended };
            members.Add(new LibraryMember
            {
                CardNumber = $"EL-{10200 + index * 7}",
                Name = name,
                Email = $"{name.ToLowerInvariant().Replace(' ', '.').Replace("ć", "c", StringComparison.Ordinal)}{(index % 5 == 0 ? index.ToString(CultureInfo.InvariantCulture) : "")}@mail.example",
                Branch = Branches[random.Next(Branches.Length)],
                Tier = (MemberTier)random.Next(4),
                Status = status,
                Joined = Today.AddDays(-random.Next(0, 3650)),
                Loans = status == MemberStatus.Active ? random.Next(0, 9) : 0,
                Fines = random.Next(100) < 22 ? Math.Round((decimal)random.NextDouble() * 18m, 2) : 0m
            });
        }
        return members;
    }
}

public static class MemberCsv
{
    /// <summary>
    /// Quotes every field and prefixes formula-like values with an apostrophe, so a spreadsheet never
    /// evaluates a member's name or email as a formula.
    /// </summary>
    public static string Export(IEnumerable<LibraryMember> members)
    {
        var csv = new StringBuilder("Card number,Name,Email,Branch,Tier,Status,Joined,Loans,Fines\n");
        foreach (var fields in members.Select(Fields))
        {
            csv.AppendJoin(',', fields.Select(Quote)).Append('\n');
        }
        return csv.ToString();
    }

    private static string[] Fields(LibraryMember member) =>
    [
        member.CardNumber, member.Name, member.Email, member.Branch, member.Tier.ToString(), member.Status.ToString(),
        member.Joined.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), member.Loans.ToString(CultureInfo.InvariantCulture),
        member.Fines.ToString("0.00", CultureInfo.InvariantCulture)
    ];

    private static string Quote(string value)
    {
        if (value.Length > 0 && "=+-@\t\r".Contains(value[0])) value = "'" + value;
        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}

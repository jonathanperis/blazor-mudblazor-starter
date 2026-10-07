using System.Globalization;
using MudBlazor;

namespace WebClient.Shared.Components.Samples.Kanban;

/// <summary>A column on the board. A limit caps work in progress; null means no limit.</summary>
public sealed record KanbanLane(string Id, string Title, int? Limit, string Icon);

public sealed record TeamMember(string Id, string Name, string Role, Color Color)
{
    public string Initials => string.Concat(Name.Split(' ').Select(part => part[0]));
}

public sealed record KanbanLabel(string Name, Color Color);

public sealed class ChecklistItem(string text, bool done = false)
{
    public string Text { get; set; } = text;
    public bool Done { get; set; } = done;
}

/// <summary>A card is mutable for the board; dialogs always edit a <see cref="Copy"/>.</summary>
public sealed class KanbanCard
{
    public int Id { get; init; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Lane { get; set; } = KanbanData.Backlog;
    public string? Assignee { get; set; }
    public IReadOnlyCollection<string> Labels { get; set; } = [];
    public DateTime? Due { get; set; }
    public int Points { get; set; } = 2;
    public List<ChecklistItem> Checklist { get; set; } = [];

    public KanbanCard Copy() => new()
    {
        Id = Id, Title = Title, Description = Description, Lane = Lane, Assignee = Assignee, Labels = Labels.ToList(),
        Due = Due, Points = Points, Checklist = Checklist.Select(item => new ChecklistItem(item.Text, item.Done)).ToList()
    };

    public void CopyFrom(KanbanCard source)
    {
        (Title, Description, Assignee, Labels, Due, Points) = (source.Title, source.Description, source.Assignee, source.Labels, source.Due, source.Points);
        Checklist = source.Checklist;
    }
}

/// <summary>What the card dialog returns: the edited copy, or a request to delete the card.</summary>
public sealed record KanbanDialogResult(KanbanCard Card, bool Delete = false);

public static class KanbanData
{
    public const string Backlog = "backlog";
    public const string InProgress = "in-progress";
    public const string Review = "review";
    public const string Done = "done";

    /// <summary>The sample's fixed "today", so overdue cards look the same on every visit.</summary>
    public static readonly DateTime Today = new(2026, 10, 9);

    public static IReadOnlyList<KanbanLane> Lanes { get; } =
    [
        new(Backlog, "Backlog", null, Icons.Material.Outlined.Inventory2),
        new(InProgress, "In progress", 3, Icons.Material.Outlined.Autorenew),
        new(Review, "Review", 2, Icons.Material.Outlined.RateReview),
        new(Done, "Done", null, Icons.Material.Outlined.TaskAlt)
    ];

    public static IReadOnlyList<TeamMember> Team { get; } =
    [
        new("amara", "Amara Okafor", "Product design", Color.Primary),
        new("jun", "Jun Park", "Mobile engineering", Color.Secondary),
        new("lucia", "Lucía Romero", "Accessibility", Color.Tertiary),
        new("tobias", "Tobias Wren", "Platform", Color.Info),
        new("priya", "Priya Nair", "Engineering lead", Color.Success)
    ];

    public static IReadOnlyList<KanbanLabel> Labels { get; } =
    [
        new("Feature", Color.Primary),
        new("Bug", Color.Error),
        new("Accessibility", Color.Secondary),
        new("Research", Color.Info),
        new("Content", Color.Tertiary)
    ];

    public static readonly int[] PointScale = [1, 2, 3, 5, 8];

    /// <summary>The board is written in English, so dates use an invariant month name.</summary>
    public static string ShortDate(DateTime date) => date.ToString("d MMM", CultureInfo.InvariantCulture);

    public static TeamMember? Member(string? id) => Team.FirstOrDefault(member => member.Id == id);
    public static Color LabelColor(string name) => Labels.FirstOrDefault(label => label.Name == name)?.Color ?? Color.Default;

    public static List<KanbanCard> Seed() =>
    [
        Card(1, "Offline timetable cache", Backlog, "jun", ["Feature"], 3, null, "Keep the next 48 hours of departures on the device so stop pages work underground.",
            ("Agree cache size with platform", false), ("Prototype on the 14 line", false)),
        Card(2, "Fare capping explainer page", Backlog, "amara", ["Content"], 2, new(2026, 10, 15), "Riders keep asking when the daily cap applies. One page, three examples, no jargon."),
        Card(3, "Investigate GPS drift near tunnels", Backlog, "tobias", ["Research", "Bug"], 3, null, "Vehicles jump 300 m when they leave the Kingsway tunnel. Find out whether to smooth on the device or the server."),
        Card(4, "Night bus map layer", Backlog, null, ["Feature"], 5, null, "Show N-routes after 23:30 with their own colour and a legend."),
        Card(5, "Real-time arrivals on stop pages", InProgress, "jun", ["Feature"], 5, new(2026, 10, 14), "Replace scheduled times with live predictions when a vehicle has reported in the last two minutes.",
            ("Subscribe to the vehicle feed", true), ("Merge predictions with the timetable", true), ("Stale-data indicator", true), ("Empty and error states", false), ("Copy review", false)),
        Card(6, "Screen reader labels for the route map", InProgress, "lucia", ["Accessibility"], 3, new(2026, 10, 8), "Every stop marker needs a name, its lines and whether it is step-free.",
            ("Audit with VoiceOver", true), ("Add labels to markers", false)),
        Card(7, "Fix duplicated alerts after reconnect", InProgress, "tobias", ["Bug"], 2, new(2026, 10, 10), "When the app reconnects, service alerts appear twice until the next refresh."),
        Card(8, "Step-free route option", Review, "priya", ["Accessibility", "Feature"], 8, new(2026, 10, 12), "Route planning that avoids stairs and out-of-service lifts.",
            ("Lift status feed", true), ("Routing weights", true), ("Toggle in planner", true)),
        Card(9, "Service alert copy refresh", Review, "amara", ["Content"], 1, new(2026, 10, 9), "Shorter alert headlines, with the affected lines first."),
        Card(10, "Bike rack availability badge", Review, "jun", ["Feature"], 2, new(2026, 10, 16), "Show free rack spaces at interchange stations."),
        Card(11, "Ticket wallet dark mode", Done, "lucia", ["Feature", "Accessibility"], 3, new(2026, 10, 7), "Barcodes stay black on white so gate scanners can read them.",
            ("Contrast check", true), ("Scanner test at Central", true)),
        Card(12, "Crash on empty favourites list", Done, "tobias", ["Bug"], 1, new(2026, 10, 6), "A new rider with no favourites saw a blank screen.")
    ];

    private static KanbanCard Card(int id, string title, string lane, string? assignee, string[] labels, int points, DateTime? due, string description, params (string Text, bool Done)[] checklist) => new()
    {
        Id = id, Title = title, Lane = lane, Assignee = assignee, Labels = labels, Points = points, Due = due, Description = description,
        Checklist = checklist.Select(item => new ChecklistItem(item.Text, item.Done)).ToList()
    };
}

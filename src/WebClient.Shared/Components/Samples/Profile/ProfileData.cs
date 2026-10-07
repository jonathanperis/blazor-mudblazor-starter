using MudBlazor;

namespace WebClient.Shared.Components.Samples.Profile;

public enum ActivityKind { Observation, Publication, Talk, Note }
public enum ProjectStatus { Planning, Observing, Analysis, Published }

public sealed record ProfileActivity(DateTime On, ActivityKind Kind, string Title, string Detail);

public sealed record ProfileProject(string Name, string Summary, ProjectStatus Status, int Progress, int Stars, string[] Tags, string[] Team);

public sealed record ProfileSkill(string Name, int Endorsements);

public sealed record Colleague(string Name, Color Color);

/// <summary>A fictional instrument scientist's public profile, with synthetic activity and projects.</summary>
public static class ProfileData
{
    public const string Name = "Imani Okafor";
    public const string Handle = "imani.okafor";
    public const string Role = "Instrument scientist";
    public const string Organisation = "Kestrel Ridge Observatory";
    public const string Location = "Brecon Beacons, Wales";
    public const int Followers = 812;
    public const int Following = 146;
    public const int Observations = 1284;
    public const int Papers = 23;

    public static IReadOnlyList<string> Bio { get; } =
    [
        "I look after the échelle spectrograph on the 0.6 m reflector and spend most clear nights chasing variable stars that refuse to keep a schedule.",
        "Before Kestrel Ridge I calibrated detectors for a balloon-borne telescope in Antarctica. Now I teach the summer school, review the queue, and argue for darker skies at county council meetings."
    ];

    public static IReadOnlyList<(string Icon, string Label, string Value)> Details { get; } =
    [
        (Icons.Material.Outlined.Work, "Works at", Organisation),
        (Icons.Material.Outlined.Place, "Based in", Location),
        (Icons.Material.Outlined.Event, "Member since", "March 2019"),
        (Icons.Material.Outlined.Translate, "Speaks", "English, Igbo, some Welsh"),
        (Icons.Material.Outlined.Schedule, "Office hours", "Thursdays 14:00–16:00, dome 2")
    ];

    public static IReadOnlyList<(string Years, string Title, string Place)> Experience { get; } =
    [
        ("2019 – now", "Instrument scientist", "Kestrel Ridge Observatory"),
        ("2015 – 2019", "Detector engineer", "Southern Lights balloon telescope"),
        ("2011 – 2015", "PhD, observational astrophysics", "University of Cardiff")
    ];

    public static IReadOnlyList<ProfileSkill> Skills { get; } =
    [
        new("Spectroscopy", 64), new("Photometry", 41), new("Adaptive optics", 18), new("Python", 57),
        new("Telescope operations", 38), new("Detector calibration", 22), new("Data pipelines", 29), new("Science outreach", 33)
    ];

    public static IReadOnlyList<Colleague> MutualFollowers { get; } =
    [
        new("Ada Lindqvist", Color.Primary), new("Rhys Pritchard", Color.Secondary), new("Mei Tanaka", Color.Tertiary),
        new("Tomás Ferreira", Color.Info), new("Hana Novak", Color.Success), new("Owen Price", Color.Warning)
    ];

    public static IReadOnlyList<ProfileActivity> Activity { get; } =
    [
        new(new DateTime(2026, 10, 6), ActivityKind.Observation, "RR Lyrae light curve, night 41", "Six hours of photometry before cloud rolled in at 03:10; the 0.57-day period holds."),
        new(new DateTime(2026, 10, 3), ActivityKind.Note, "Spectrograph focus drift", "Logged a 12 µm drift after the dome heater cycled. Workaround pinned for the queue."),
        new(new DateTime(2026, 9, 28), ActivityKind.Publication, "Accepted: “Blue loops in nearby Cepheids”", "Second author, with M. Tanaka and the Ridge variable star group."),
        new(new DateTime(2026, 9, 21), ActivityKind.Talk, "Dark skies, bright towns", "Public talk at Brecon library, 140 people, three new volunteers."),
        new(new DateTime(2026, 9, 14), ActivityKind.Observation, "Comet C/2026 F2 astrometry", "Twelve positions sent to the Minor Planet Center within the hour."),
        new(new DateTime(2026, 9, 2), ActivityKind.Note, "New flat-field routine", "Twilight flats now take 9 minutes instead of 25. Script in the shared notebook."),
        new(new DateTime(2026, 8, 19), ActivityKind.Publication, "Data release: Ridge Variable Survey DR3", "4,812 light curves, calibrated and public.")
    ];

    public static IReadOnlyList<ProfileProject> Projects { get; } =
    [
        new("Ridge Variable Survey", "Nightly photometry of 5,000 variable stars brighter than magnitude 14.", ProjectStatus.Observing, 72, 148, ["Photometry", "Long-term"], ["Imani Okafor", "Mei Tanaka", "Owen Price"]),
        new("Échelle upgrade", "A new fibre feed and thermal enclosure for the spectrograph.", ProjectStatus.Analysis, 45, 61, ["Instrumentation"], ["Imani Okafor", "Rhys Pritchard"]),
        new("Cepheid blue loops", "Radial velocities for 40 Cepheids to test stellar evolution models.", ProjectStatus.Published, 100, 203, ["Spectroscopy", "Paper"], ["Mei Tanaka", "Imani Okafor", "Hana Novak", "Tomás Ferreira"]),
        new("Dark sky reserve bid", "Light-meter readings across the valley for the reserve application.", ProjectStatus.Planning, 15, 34, ["Outreach", "Community"], ["Imani Okafor", "Ada Lindqvist"])
    ];

    public static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));

    public static (string Icon, Color Color) Style(ActivityKind kind) => kind switch
    {
        ActivityKind.Observation => (Icons.Material.Outlined.Visibility, Color.Primary),
        ActivityKind.Publication => (Icons.Material.Outlined.Article, Color.Secondary),
        ActivityKind.Talk => (Icons.Material.Outlined.RecordVoiceOver, Color.Tertiary),
        _ => (Icons.Material.Outlined.EditNote, Color.Info)
    };

    public static Color StatusColor(ProjectStatus status) => status switch
    {
        ProjectStatus.Observing => Color.Primary,
        ProjectStatus.Analysis => Color.Info,
        ProjectStatus.Published => Color.Success,
        _ => Color.Default
    };
}

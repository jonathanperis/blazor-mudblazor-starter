using System.Net.Mail;
using MudBlazor;

namespace WebClient.Shared.Components.Samples.Settings;

public enum SettingsSection { Profile, Notifications, Appearance, Security }
public enum ThemeChoice { Light, Dark, System }
public enum DigestFrequency { Never, Daily, Weekly }
public enum LayoutDensity { Comfortable, Compact }

/// <summary>
/// Everything the save button commits. A record gives value equality and <c>with</c> copies, so the page edits a
/// draft copy and compares it with the saved copy to know what is unsaved.
/// </summary>
public sealed record AccountSettings
{
    public string FullName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public string Bio { get; set; } = "";
    public bool NotifyMentions { get; set; }
    public bool NotifyShiftSwaps { get; set; }
    public bool NotifyDeliveries { get; set; }
    public bool PushOvenTimers { get; set; }
    public bool QuietHours { get; set; }
    public DigestFrequency Digest { get; set; }
    public ThemeChoice Theme { get; set; }
    public Color Accent { get; set; }
    public LayoutDensity Density { get; set; }
    public bool ReduceMotion { get; set; }
    public bool TwoFactor { get; set; }
}

/// <summary>A signed-in device. Signing one out happens immediately; it is not part of the draft.</summary>
public sealed record ActiveSession(int Id, string Device, string Browser, string Place, string LastActive, string Icon, bool IsCurrent);

/// <summary>The fictional account behind the settings sample, and the rules the save button checks.</summary>
public static class SettingsData
{
    public const int BioLimit = 160;

    public static AccountSettings Initial() => new()
    {
        FullName = "Inês Carvalho",
        DisplayName = "Inês",
        Email = "ines@larkspur.example",
        TimeZone = "Europe/London",
        Bio = "Head baker at Mill Lane. Laminated dough, long ferments and teaching the 4 a.m. shift.",
        NotifyMentions = true,
        NotifyShiftSwaps = true,
        NotifyDeliveries = false,
        PushOvenTimers = true,
        QuietHours = true,
        Digest = DigestFrequency.Weekly,
        Theme = ThemeChoice.System,
        Accent = Color.Primary,
        Density = LayoutDensity.Comfortable,
        TwoFactor = true
    };

    public static IReadOnlyList<string> TimeZones { get; } = ["Europe/London", "Europe/Lisbon", "Europe/Paris", "America/New_York", "Asia/Tokyo"];

    public static IReadOnlyList<(Color Color, string Name)> Accents { get; } = [(Color.Primary, "Vermilion"), (Color.Secondary, "Teal"), (Color.Tertiary, "Ochre")];

    public static IReadOnlyList<ActiveSession> Sessions { get; } =
    [
        new(1, "MacBook Air", "Firefox 131", "Ashby Vale, UK", "Active now", Icons.Material.Outlined.Laptop, true),
        new(2, "Pixel 8", "Larkspur app", "Ashby Vale, UK", "12 minutes ago", Icons.Material.Outlined.PhoneAndroid, false),
        new(3, "Bakery tablet", "Chrome 129", "Mill Lane shop", "Yesterday, 05:12", Icons.Material.Outlined.Tablet, false),
        new(4, "Windows desktop", "Edge 129", "Lisbon, Portugal", "3 weeks ago", Icons.Material.Outlined.DesktopWindows, false)
    ];

    public static string? ValidateName(string? name) => string.IsNullOrWhiteSpace(name) ? "Your name appears on rotas, so it can't be empty." : null;

    public static string? ValidateEmail(string? email) =>
        MailAddress.TryCreate(email?.Trim() ?? "", out var address) && address.Host.Contains('.') ? null : "Enter an email like name@example.org.";

    public static string? ValidateBio(string? bio) => bio?.Length > BioLimit ? $"Keep it under {BioLimit} characters." : null;

    /// <summary>Problems that block saving, checked on the whole draft so hidden sections count too.</summary>
    public static IEnumerable<string> Problems(AccountSettings draft) =>
        new[] { ValidateName(draft.FullName), ValidateEmail(draft.Email), ValidateBio(draft.Bio) }.OfType<string>();

    /// <summary>The sections whose values differ between two copies, for the unsaved-changes markers.</summary>
    public static IReadOnlySet<SettingsSection> ChangedSections(AccountSettings saved, AccountSettings draft)
    {
        var changed = new HashSet<SettingsSection>();
        if ((saved.FullName, saved.DisplayName, saved.Email, saved.TimeZone, saved.Bio) != (draft.FullName, draft.DisplayName, draft.Email, draft.TimeZone, draft.Bio))
            changed.Add(SettingsSection.Profile);
        if ((saved.NotifyMentions, saved.NotifyShiftSwaps, saved.NotifyDeliveries, saved.PushOvenTimers, saved.QuietHours, saved.Digest) != (draft.NotifyMentions, draft.NotifyShiftSwaps, draft.NotifyDeliveries, draft.PushOvenTimers, draft.QuietHours, draft.Digest))
            changed.Add(SettingsSection.Notifications);
        if ((saved.Theme, saved.Accent, saved.Density, saved.ReduceMotion) != (draft.Theme, draft.Accent, draft.Density, draft.ReduceMotion))
            changed.Add(SettingsSection.Appearance);
        if (saved.TwoFactor != draft.TwoFactor)
            changed.Add(SettingsSection.Security);
        return changed;
    }

    public static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));

    /// <summary>"Profile", "Profile and Security", "Profile, Appearance and Security".</summary>
    public static string Sentence(IEnumerable<SettingsSection> sections)
    {
        var names = sections.Order().Select(section => section.ToString()).ToList();
        return names.Count < 2 ? string.Concat(names) : $"{string.Join(", ", names[..^1])} and {names[^1]}";
    }

    public static string Icon(SettingsSection section) => section switch
    {
        SettingsSection.Profile => Icons.Material.Outlined.Person,
        SettingsSection.Notifications => Icons.Material.Outlined.Notifications,
        SettingsSection.Appearance => Icons.Material.Outlined.Palette,
        _ => Icons.Material.Outlined.Shield
    };
}

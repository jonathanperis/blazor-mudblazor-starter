using System.Globalization;
using MudBlazor;

namespace WebClient.Shared.Components.Samples.Inbox;

public enum MailFolder { Inbox, Sent, Drafts, Archive, Trash }

public enum InboxAction { Archive, Delete, MarkRead, MarkUnread, MoveToInbox, Star, Forward }

/// <summary>One message in the night-operations mailbox of a fictional observatory.</summary>
public sealed class InboxMessage
{
    public int Id { get; init; }
    public string From { get; init; } = "";
    public string Address { get; init; } = "";
    public string To { get; init; } = "Maren Holt";
    public string Subject { get; init; } = "";
    public string[] Body { get; init; } = [];
    public DateTime Received { get; init; }
    public string[] Labels { get; init; } = [];
    public string[] Attachments { get; init; } = [];
    public MailFolder Folder { get; set; } = MailFolder.Inbox;
    public bool Unread { get; set; }
    public bool Starred { get; set; }

    public string Preview => Body.Length == 0 ? "" : Body[0];
    public string Initials => string.Concat(From.Split(' ').Where(part => part.Length > 0 && char.IsLetter(part[0])).Take(2).Select(part => part[0]));
}

/// <summary>A place in the mailbox: a real folder, the starred view, or a label.</summary>
public sealed record MailView(string Key, string Title, string Icon, Func<InboxMessage, bool> Contains);

public static class InboxData
{
    /// <summary>The sample's fixed "now", so relative dates read the same on every visit.</summary>
    public static readonly DateTime Now = new(2026, 10, 9, 8, 40, 0);

    public static readonly string[] LabelNames = ["Observing", "Instruments", "Outreach"];

    public static IReadOnlyList<MailView> Views { get; } =
    [
        new("inbox", "Inbox", Icons.Material.Outlined.Inbox, message => message.Folder == MailFolder.Inbox),
        new("starred", "Starred", Icons.Material.Outlined.StarBorder, message => message.Starred && message.Folder != MailFolder.Trash),
        new("sent", "Sent", Icons.Material.Outlined.Send, message => message.Folder == MailFolder.Sent),
        new("drafts", "Drafts", Icons.Material.Outlined.Drafts, message => message.Folder == MailFolder.Drafts),
        new("archive", "Archive", Icons.Material.Outlined.Archive, message => message.Folder == MailFolder.Archive),
        new("trash", "Trash", Icons.Material.Outlined.Delete, message => message.Folder == MailFolder.Trash),
        .. LabelNames.Select(label => new MailView($"label-{label.ToLowerInvariant()}", label, Icons.Material.Outlined.Label,
            message => message.Labels.Contains(label) && message.Folder != MailFolder.Trash))
    ];

    /// <summary>Search matches: a tint of the theme's ochre instead of the browser's yellow, readable in both themes.</summary>
    public const string MarkStyle = "background: color-mix(in srgb, var(--mud-palette-tertiary) 32%, transparent); color: inherit; border-radius: 2px; padding: 0 1px";

    public static Color LabelColor(string label) => label switch
    {
        "Observing" => Color.Info,
        "Instruments" => Color.Tertiary,
        _ => Color.Secondary
    };

    /// <summary>MudBlazor's text color utility class for a label, such as <c>mud-info-text</c>.</summary>
    public static string LabelClass(string label) => $"mud-{LabelColor(label).ToString().ToLowerInvariant()}-text";

    /// <summary>Time today, weekday this week, otherwise day and month.</summary>
    public static string When(DateTime received) =>
        received.Date == Now.Date ? received.ToString("HH:mm", CultureInfo.InvariantCulture)
        : (Now.Date - received.Date).TotalDays < 6 ? received.ToString("ddd", CultureInfo.InvariantCulture)
        : received.ToString("d MMM", CultureInfo.InvariantCulture);

    public static string FullDate(DateTime received) => received.ToString("dddd d MMMM yyyy, HH:mm", CultureInfo.InvariantCulture);

    public static List<InboxMessage> Seed() =>
    [
        new()
        {
            Id = 1, From = "Ravi Desai", Address = "ravi.desai@hollowhill.example", Subject = "Dome 2 shutter fault, logged at 03:12",
            Received = Now.AddMinutes(-28), Unread = true, Starred = true, Labels = ["Instruments"], Attachments = ["shutter-log-0312.txt"],
            Body =
            [
                "The east shutter on dome 2 stopped at 40% during the 03:00 close. The motor drew normal current, so I suspect the limit switch rather than the drive.",
                "I closed it by hand and secured the dome before the cloud came in. Nothing got wet, and the 0.6 m stayed parked the whole time.",
                "Could you keep dome 2 off tonight's schedule until I have checked the switch this afternoon? Log attached."
            ]
        },
        new()
        {
            Id = 2, From = "Night allocation committee", Address = "allocation@hollowhill.example", Subject = "Semester B time allocation: your proposal was accepted",
            Received = Now.AddHours(-2.5), Unread = true, Labels = ["Observing"],
            Body =
            [
                "We are pleased to confirm that proposal HH-2026B-014, \"Variability of dusty white dwarfs\", has been allocated 11 nights on the 0.6 m telescope.",
                "Nights are spread across November and December to cover the lunar cycle as requested. The full schedule will be published on Friday.",
                "Please confirm the observer for each block by 20 October."
            ]
        },
        new()
        {
            Id = 3, From = "Ana Lucía Ferreyra", Address = "ana.ferreyra@hollowhill.example", Subject = "School visit on the 22nd: telescope demo plan",
            Received = Now.AddHours(-5), Unread = true, Labels = ["Outreach"],
            Body =
            [
                "Thirty-two pupils from Millbrook Primary are coming on the 22nd. They have been learning about the Moon, so a first-quarter Moon is perfect timing.",
                "I would like to use the small refractor on the lawn plus the live feed from dome 1 on the big screen. Is dome 1 free between 18:30 and 20:00?",
                "If the weather turns, I will run the planetarium show instead. Happy to take any suggestions."
            ]
        },
        new()
        {
            Id = 4, From = "Ravi Desai", Address = "ravi.desai@hollowhill.example", Subject = "Spectrograph recalibration finished",
            Received = Now.AddDays(-1).AddHours(-3), Labels = ["Instruments"], Attachments = ["arc-lamp-fit.pdf", "residuals.csv"],
            Body =
            [
                "The arc-lamp fit is done. RMS residuals are down to 0.04 Å across the full range, a clear improvement on last month.",
                "The new wavelength solution is in the pipeline config, version 2026.10.1. Old frames will keep the solution they were reduced with."
            ]
        },
        new()
        {
            Id = 5, From = "Weather station", Address = "wx@hollowhill.example", Subject = "Forecast: clear from 21:00, seeing about 1.4 arcseconds",
            Received = Now.AddDays(-1).AddHours(-6), Labels = ["Observing"],
            Body =
            [
                "Clear skies expected from 21:00 until 04:30. Humidity below 70% all night, wind north-easterly at 10–15 km/h.",
                "Predicted seeing 1.3–1.5 arcseconds. Good conditions for the photometry queue."
            ]
        },
        new()
        {
            Id = 6, From = "Grace Mwangi", Address = "grace.mwangi@hollowhill.example", Subject = "Visitor centre rota for October",
            Received = Now.AddDays(-2).AddHours(-1), Unread = true, Labels = ["Outreach"],
            Body =
            [
                "The October rota is attached. Two Saturday evenings still need someone who can run the telescope tour.",
                "If you can cover the 17th or the 31st, reply and I will put your name down."
            ],
            Attachments = ["visitor-rota-october.xlsx"]
        },
        new()
        {
            Id = 7, From = "Facilities", Address = "facilities@hollowhill.example", Subject = "Generator test on Thursday morning",
            Received = Now.AddDays(-3).AddHours(-2),
            Body =
            [
                "The backup generator will be tested on Thursday between 07:00 and 07:30. Power to the domes will switch over twice.",
                "Please make sure all instruments are parked and powered down before 07:00."
            ]
        },
        new()
        {
            Id = 8, From = "Tomasz Krawczyk", Address = "t.krawczyk@university.example", Subject = "Data request: photometry of V1405 Cas",
            Received = Now.AddDays(-4).AddHours(-4), Starred = true, Labels = ["Observing"],
            Body =
            [
                "Our group is modelling the late decline of the nova V1405 Cas and would love to include your V-band photometry from August.",
                "We would of course credit the observatory and the observers in the paper. Reduced magnitudes are enough; we do not need the raw frames."
            ]
        },
        new()
        {
            Id = 9, From = "Ravi Desai", Address = "ravi.desai@hollowhill.example", Subject = "Filter wheel spare parts arrived",
            Received = Now.AddDays(-6), Labels = ["Instruments"],
            Body = ["The replacement detent springs for the filter wheel arrived. I will fit them during the next cloudy night."]
        },
        new()
        {
            Id = 10, From = "Ana Lucía Ferreyra", Address = "ana.ferreyra@hollowhill.example", Subject = "Thank you from the astronomy club",
            Received = Now.AddDays(-9), Labels = ["Outreach"], Folder = MailFolder.Archive,
            Body = ["The club asked me to pass on their thanks for the Saturn night. Several of them have asked when they can come back."]
        },
        new()
        {
            Id = 11, From = "Maren Holt", Address = "maren.holt@hollowhill.example", To = "Ravi Desai", Subject = "Re: Guider camera noise",
            Received = Now.AddDays(-5), Folder = MailFolder.Sent, Labels = ["Instruments"],
            Body = ["Thanks for checking. Let's swap the USB cable first before we send the camera away; it is the cheapest thing to rule out."]
        },
        new()
        {
            Id = 12, From = "Maren Holt", Address = "maren.holt@hollowhill.example", To = "Night allocation committee", Subject = "Observer list for semester B",
            Received = Now.AddHours(-1), Folder = MailFolder.Drafts,
            Body = ["Block 1 (3–6 November): M. Holt and R. Desai. Block 2: to be confirmed."]
        },
        new()
        {
            Id = 13, From = "Newsletter", Address = "news@telescope-supplies.example", Subject = "Autumn sale on eyepieces",
            Received = Now.AddDays(-7), Folder = MailFolder.Trash,
            Body = ["Twenty percent off every eyepiece until the end of the month."]
        }
    ];
}

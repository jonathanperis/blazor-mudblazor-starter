using System.Globalization;

namespace WebClient.Shared.Components.Samples.Pricing;

public enum Billing { Monthly, Yearly }

/// <summary>A plan of the fictional Fieldnote lab notebook. Prices are per seat per month, before any yearly discount.</summary>
public sealed record PricingPlan(string Id, string Name, string Tagline, decimal MonthlyPrice, int? MaxSeats, bool Featured, string[] Highlights);

/// <summary>A row of the comparison table: one value per plan, where <c>"yes"</c> and <c>""</c> mean included and not included.</summary>
public sealed record PlanFeature(string Group, string Name, string Bench, string Lab, string Institute)
{
    public string For(string planId) => planId switch { "bench" => Bench, "lab" => Lab, _ => Institute };
}

public static class PricingData
{
    public const decimal YearlyDiscount = 0.20m;
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

    public static IReadOnlyList<PricingPlan> Plans { get; } =
    [
        new("bench", "Bench", "For a student project or a small side study.", 0m, 3, false,
            ["Up to 3 people", "3 shared notebooks", "1 GB of attachments", "7 days of version history", "PDF and Markdown export"]),
        new("lab", "Lab", "For research groups that run experiments every week.", 9m, null, true,
            ["Unlimited notebooks", "50 GB per seat", "A year of version history", "Protocol templates and sample tracker", "Roles for PIs, students and guests"]),
        new("institute", "Institute", "For departments with compliance and audit needs.", 19m, null, false,
            ["Everything in Lab", "Single sign-on with your university", "Signed entries and a full audit trail", "Choose UK or EU data residency", "Named support engineer"])
    ];

    public static IReadOnlyList<PlanFeature> Features { get; } =
    [
        new("Notebooks", "Shared notebooks", "3", "Unlimited", "Unlimited"),
        new("Notebooks", "Attachment storage", "1 GB", "50 GB / seat", "200 GB / seat"),
        new("Notebooks", "Version history", "7 days", "1 year", "Forever"),
        new("Notebooks", "Protocol templates", "", "yes", "yes"),
        new("Collaboration", "Comments and mentions", "yes", "yes", "yes"),
        new("Collaboration", "Sample and reagent tracker", "", "yes", "yes"),
        new("Collaboration", "Guest reviewers", "", "5", "Unlimited"),
        new("Security", "Two-step verification", "yes", "yes", "yes"),
        new("Security", "Single sign-on (SAML)", "", "", "yes"),
        new("Security", "Signed entries and audit trail", "", "", "yes"),
        new("Support", "Help centre and community", "yes", "yes", "yes"),
        new("Support", "Email support", "", "Next working day", "4 hours"),
        new("Support", "Uptime commitment", "", "99.5 %", "99.9 %")
    ];

    public static IReadOnlyList<(string Question, string Answer)> Questions { get; } =
    [
        ("Can I try Lab or Institute before paying?", "Yes. Every paid plan starts with a 30-day trial for your whole team, with no card required. At the end you choose a plan or drop back to Bench; nothing is deleted."),
        ("What counts as a seat?", "Anyone who can edit a notebook. Guest reviewers who only read and comment are free, up to the limit of your plan."),
        ("How does yearly billing work?", "You pay for twelve months up front at 20 % less than the monthly price. Seats you add during the year are charged pro rata on your next invoice."),
        ("Do you offer academic or charity discounts?", "Teaching labs and registered charities get Lab at half price. Write to us from your institution's email address."),
        ("Where is our data stored?", "Bench and Lab data lives in London. Institute lets you pin each workspace to the UK or the EU, and you can export everything at any time.")
    ];

    public static decimal PerSeat(PricingPlan plan, Billing billing) =>
        billing == Billing.Yearly ? decimal.Round(plan.MonthlyPrice * (1 - YearlyDiscount), 2) : plan.MonthlyPrice;

    public static decimal MonthlyTotal(PricingPlan plan, Billing billing, int seats) => PerSeat(plan, billing) * seats;

    public static string Money(decimal value) => value % 1 == 0 ? value.ToString("C0", Culture) : value.ToString("C2", Culture);
}

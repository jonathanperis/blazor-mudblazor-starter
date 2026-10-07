using System.Globalization;
using MudBlazor;

namespace WebClient.Shared.Components.Samples.Dashboard;

/// <summary>A shop in the fictional Larkspur Bakehouse chain.</summary>
public sealed record BakeryShop(string Id, string Name, string Town, decimal QuarterTarget);

/// <summary>One shop's trading day.</summary>
public sealed record TradingDay(DateTime Day, string ShopId, decimal Revenue, int Orders, int LoavesBaked, int LoavesWasted);

/// <summary>A product line and how its share of sales moves through the year.</summary>
public sealed record BakeryProduct(string Name, string Category, decimal Price, double BaseShare, int PeakMonth, double Seasonality);

/// <summary>A row in the best sellers table, computed for the selected range.</summary>
public sealed record ProductSales(int Rank, string Name, string Category, int Units, decimal Revenue, double Share);

/// <summary>Something that happened in a shop, shown on the activity timeline.</summary>
public sealed record ShopActivity(DateTime At, string ShopId, string Title, string Detail, string Icon, Color Color);

/// <summary>Quick ranges offered by the period toggle; a range picked on the calendar has no preset.</summary>
public enum DashboardPeriod { Week, Month, Quarter, QuarterToDate }

/// <summary>Totals for one date range.</summary>
public sealed record PeriodTotals(decimal Revenue, int Orders, int LoavesBaked, int LoavesWasted)
{
    public decimal AverageBasket => Orders == 0 ? 0 : Revenue / Orders;
    public double WasteRate => LoavesBaked == 0 ? 0 : (double)LoavesWasted / LoavesBaked;
}

/// <summary>
/// Deterministic synthetic trading data: a year of days for six shops, generated from a fixed seed so every visit,
/// test and screenshot sees the same numbers.
/// </summary>
public static class DashboardData
{
    /// <summary>The dashboard's "today". Data runs up to and including this day.</summary>
    public static readonly DateTime Today = new(2026, 9, 18);
    public static readonly DateTime QuarterStart = new(2026, 7, 1);
    public static readonly DateTime QuarterEnd = new(2026, 9, 30);
    /// <summary>Earliest day the range picker offers; a full previous period always exists before it.</summary>
    public static readonly DateTime FirstSelectableDay = Today.AddDays(-179);
    public const int MaxRangeDays = 92;
    public const string AllShops = "all";

    /// <summary>The chain trades in pounds; dates and money use British formatting regardless of the UI culture.</summary>
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-GB");

    public static IReadOnlyList<BakeryShop> Shops { get; } =
    [
        new("mill", "Mill Lane", "Ashby Vale", 300_000m),
        new("quay", "The Quay", "Harrowgate", 262_000m),
        new("market", "Market Cross", "Ashby Vale", 340_000m),
        new("station", "Station Arcade", "Fernley", 236_000m),
        new("orchard", "Orchard Row", "Little Brampton", 182_000m),
        new("bridge", "Bridge Street", "Harrowgate", 258_000m)
    ];

    public static IReadOnlyList<BakeryProduct> Products { get; } =
    [
        new("Country sourdough", "Bread", 4.80m, 0.19, 1, 0.05),
        new("Cardamom bun", "Pastry", 3.20m, 0.15, 11, 0.20),
        new("Seeded rye", "Bread", 4.40m, 0.10, 2, 0.08),
        new("Butter croissant", "Pastry", 2.90m, 0.16, 6, 0.06),
        new("Lemon tart", "Cake", 3.90m, 0.07, 7, 0.45),
        new("Pumpkin loaf", "Cake", 4.20m, 0.04, 10, 0.80),
        new("Filter coffee", "Drinks", 2.60m, 0.18, 1, 0.10),
        new("Iced oat latte", "Drinks", 3.60m, 0.06, 7, 0.70)
    ];

    public static IReadOnlyList<TradingDay> Days { get; } = Generate();

    public static IReadOnlyList<ShopActivity> Activity { get; } =
    [
        new(new DateTime(2026, 9, 18, 7, 42, 0), "market", "Ovens on at Market Cross", "First bake out 12 minutes early; 418 loaves planned.", Icons.Material.Outlined.LocalFireDepartment, Color.Primary),
        new(new DateTime(2026, 9, 17, 16, 5, 0), "quay", "Record Thursday at The Quay", "£3,912 taken, beating the previous best by £240.", Icons.Material.Outlined.EmojiEvents, Color.Success),
        new(new DateTime(2026, 9, 16, 9, 30, 0), "station", "Flour delivery late", "Stoneground rye arrived 3 hours late; seeded rye sold out by noon.", Icons.Material.Outlined.LocalShipping, Color.Warning),
        new(new DateTime(2026, 9, 14, 11, 0, 0), "all", "Pumpkin loaf launched", "Autumn menu live in every shop, priced at £4.20.", Icons.Material.Outlined.NewReleases, Color.Secondary),
        new(new DateTime(2026, 9, 11, 14, 20, 0), "orchard", "Proofer repaired", "Orchard Row back to full capacity after two short days.", Icons.Material.Outlined.Build, Color.Info),
        new(new DateTime(2026, 9, 6, 8, 15, 0), "bridge", "Saturday market stall", "Bridge Street sold 140 cardamom buns at the riverside market.", Icons.Material.Outlined.Storefront, Color.Tertiary),
        new(new DateTime(2026, 8, 29, 17, 45, 0), "mill", "Waste below 3 %", "Mill Lane's evening markdown cut unsold bread to 2.8 %.", Icons.Material.Outlined.Recycling, Color.Success),
        new(new DateTime(2026, 8, 21, 10, 10, 0), "market", "Card reader outage", "Cash only for 40 minutes; about 30 orders estimated lost.", Icons.Material.Outlined.CreditCardOff, Color.Error),
        new(new DateTime(2026, 8, 12, 6, 55, 0), "all", "Summer opening hours", "All shops open at 7:00 until the end of August.", Icons.Material.Outlined.Schedule, Color.Info),
        new(new DateTime(2026, 7, 30, 15, 0, 0), "quay", "New head baker", "Priya Raman joins The Quay from the Ashby Vale bakery school.", Icons.Material.Outlined.PersonAdd, Color.Secondary),
        new(new DateTime(2026, 7, 14, 12, 0, 0), "station", "Heatwave menu", "Iced drinks added at Station Arcade; tarts moved to the chiller.", Icons.Material.Outlined.WbSunny, Color.Warning),
        new(new DateTime(2026, 6, 26, 9, 0, 0), "orchard", "Orchard Row turns two", "Anniversary weekend with a free bun for every 50th customer.", Icons.Material.Outlined.Cake, Color.Tertiary)
    ];

    public static string Currency(decimal value) => value.ToString("C0", Culture);
    public static string Currency2(decimal value) => value.ToString("C2", Culture);
    public static string Date(DateTime value, string format) => value.ToString(format, Culture);
    public static string ShopName(string shopId) => Shops.FirstOrDefault(shop => shop.Id == shopId)?.Name ?? "All shops";

    public static (DateTime Start, DateTime End) RangeFor(DashboardPeriod period) => period switch
    {
        DashboardPeriod.Week => (Today.AddDays(-6), Today),
        DashboardPeriod.Month => (Today.AddDays(-29), Today),
        DashboardPeriod.Quarter => (Today.AddDays(-89), Today),
        _ => (QuarterStart, Today)
    };

    public static IEnumerable<TradingDay> For(string shopId, DateTime start, DateTime end) =>
        Days.Where(day => day.Day >= start && day.Day <= end && (shopId == AllShops || day.ShopId == shopId));

    public static PeriodTotals Totals(IEnumerable<TradingDay> days)
    {
        decimal revenue = 0; int orders = 0, baked = 0, wasted = 0;
        foreach (var day in days)
        {
            revenue += day.Revenue; orders += day.Orders; baked += day.LoavesBaked; wasted += day.LoavesWasted;
        }
        return new PeriodTotals(revenue, orders, baked, wasted);
    }

    public static PeriodTotals Combine(IEnumerable<PeriodTotals> parts) => parts.Aggregate(new PeriodTotals(0, 0, 0, 0),
        (sum, part) => new PeriodTotals(sum.Revenue + part.Revenue, sum.Orders + part.Orders, sum.LoavesBaked + part.LoavesBaked, sum.LoavesWasted + part.LoavesWasted));

    /// <summary>One average day of a bucket, so a short final week compares fairly with full ones.</summary>
    public static PeriodTotals AveragePerDay(IReadOnlyList<PeriodTotals> days)
    {
        var sum = Combine(days);
        return new PeriodTotals(sum.Revenue / days.Count, sum.Orders / days.Count, sum.LoavesBaked / days.Count, sum.LoavesWasted / days.Count);
    }

    /// <summary>Daily totals across the selected shops, one entry per calendar day in the range.</summary>
    public static IReadOnlyList<(DateTime Day, PeriodTotals Totals)> Daily(string shopId, DateTime start, DateTime end) =>
        For(shopId, start, end).GroupBy(day => day.Day).OrderBy(group => group.Key).Select(group => (group.Key, Totals(group))).ToList();

    /// <summary>Best sellers: each day's orders split across products by their seasonal share.</summary>
    public static IReadOnlyList<ProductSales> BestSellers(string shopId, DateTime start, DateTime end)
    {
        var units = new double[Products.Count];
        foreach (var day in For(shopId, start, end))
        {
            var shares = Products.Select(product => SeasonalShare(product, day.Day.Month)).ToArray();
            var total = shares.Sum();
            for (var i = 0; i < units.Length; i++) units[i] += day.Orders * 1.7 * shares[i] / total;
        }
        var revenue = Products.Select((product, i) => product.Price * (decimal)units[i]).ToArray();
        var allRevenue = revenue.Sum();
        return Products
            .Select((product, i) => (product, units: (int)Math.Round(units[i]), revenue: revenue[i]))
            .OrderByDescending(item => item.revenue)
            .Select((item, index) => new ProductSales(index + 1, item.product.Name, item.product.Category, item.units, decimal.Round(item.revenue), allRevenue == 0 ? 0 : (double)(item.revenue / allRevenue)))
            .ToList();
    }

    private static double SeasonalShare(BakeryProduct product, int month)
    {
        // Distance in months from the product's best month, wrapping around the year.
        var distance = Math.Min(Math.Abs(month - product.PeakMonth), 12 - Math.Abs(month - product.PeakMonth));
        return product.BaseShare * (1 + product.Seasonality * Math.Cos(distance * Math.PI / 6));
    }

    private static List<TradingDay> Generate()
    {
        var random = new Random(1847);
        var baseRevenue = new Dictionary<string, double> { ["mill"] = 3300, ["quay"] = 2900, ["market"] = 3700, ["station"] = 2300, ["orchard"] = 1750, ["bridge"] = 2550 };
        double[] weekday = [1.18, 0.82, 0.88, 0.92, 0.97, 1.08, 1.34]; // Sunday first, as DayOfWeek is.
        var days = new List<TradingDay>();
        for (var day = Today.AddDays(-364); day <= Today; day = day.AddDays(1))
        {
            var trend = 1 + (day - Today).TotalDays * 0.0007;
            var season = 1 + 0.07 * Math.Cos((day.DayOfYear - 200) * 2.0 * Math.PI / 365);
            foreach (var shop in Shops)
            {
                var revenue = baseRevenue[shop.Id] * weekday[(int)day.DayOfWeek] * trend * season * (0.92 + random.NextDouble() * 0.16);
                var basket = 11.2 + random.NextDouble() * 1.6;
                var baked = (int)(revenue / 9.5);
                var wasteRate = 0.025 + random.NextDouble() * 0.045 + (day.DayOfWeek == DayOfWeek.Monday ? 0.015 : 0);
                days.Add(new TradingDay(day, shop.Id, decimal.Round((decimal)revenue, 2), (int)(revenue / basket), baked, (int)(baked * wasteRate)));
            }
        }
        return days;
    }
}

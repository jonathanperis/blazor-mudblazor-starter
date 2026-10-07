using System.Globalization;
using System.Text.RegularExpressions;
using MudBlazor;

namespace WebClient.Shared.Components.Samples.Checkout;

/// <summary>A tea from Kettle &amp; Leaf, a fictional tea shop. Tins are drawn as inline SVG.</summary>
public sealed record Tea(string Id, string Name, string Origin, int Grams, decimal Price, string Tin, string Band);

public sealed class CartLine(Tea tea, int quantity)
{
    public const int MaxQuantity = 12;
    public Tea Tea { get; } = tea;
    public int Quantity { get; set; } = quantity;
    public decimal Total => Tea.Price * Quantity;
}

public sealed record DeliveryOption(string Id, string Title, string Detail, decimal Price, string Icon, int WorkingDays);

public sealed class ShippingAddress
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Line1 { get; set; } = "";
    public string? Line2 { get; set; }
    public string Town { get; set; } = "";
    public string Postcode { get; set; } = "";
}

public sealed class PaymentCard
{
    public string Holder { get; set; } = "";
    public string Number { get; set; } = "";
    public string Expiry { get; set; } = "";
    public string Cvc { get; set; } = "";
    public string LastFour => Digits(Number) is { Length: >= 4 } digits ? digits[^4..] : "";
    public static string Digits(string? text) => new((text ?? "").Where(char.IsAsciiDigit).ToArray());
}

/// <summary>Prices include VAT at 20%, so the VAT share of a total is one sixth of it.</summary>
public sealed record OrderTotals(decimal Subtotal, decimal Discount, decimal DeliveryPrice)
{
    public decimal Total => Subtotal - Discount + DeliveryPrice;
    public decimal Vat => Math.Round(Total / 6, 2);

    public static OrderTotals For(IEnumerable<CartLine> lines, bool promoApplied, DeliveryOption? delivery)
    {
        var subtotal = lines.Sum(line => line.Total);
        return new OrderTotals(subtotal, promoApplied ? Math.Round(subtotal * CheckoutData.PromoRate, 2) : 0, delivery?.Price ?? 0);
    }
}

public static partial class CheckoutData
{
    public const string PromoCode = "LEAF10";
    public const decimal PromoRate = .10m;

    /// <summary>The sample's fixed "today", so card expiry checks behave the same on every visit.</summary>
    public static readonly DateTime Today = new(2026, 10, 9);

    public static IReadOnlyList<Tea> Teas { get; } =
    [
        new("darjeeling", "First Flush Darjeeling", "Margaret's Hope, India", 100, 14.50m, "#2F5D50", "#E9D8A6"),
        new("sencha", "Uji Sencha", "Kyoto, Japan", 80, 12.00m, "#6B8F3A", "#F2EFD9"),
        new("hojicha", "Roasted Hojicha", "Shizuoka, Japan", 80, 9.75m, "#7A4A2A", "#F0D9C0"),
        new("breakfast", "Weaver's Row Breakfast", "Assam and Ceylon blend", 250, 8.50m, "#B4441F", "#FBEFE6")
    ];

    public static List<CartLine> StartingCart() => [new(Teas[0], 1), new(Teas[2], 2), new(Teas[3], 1)];

    public static IReadOnlyList<DeliveryOption> DeliveryOptions { get; } =
    [
        new("standard", "Standard", "Royal Mail tracked, 3–5 working days", 3.95m, Icons.Material.Outlined.LocalShipping, 5),
        new("express", "Express", "Next working day when ordered by 2 pm", 7.50m, Icons.Material.Outlined.Bolt, 1),
        new("collect", "Collect from the shop", "8 Cutler Street, ready in two hours", 0m, Icons.Material.Outlined.Storefront, 0)
    ];

    public static string Money(decimal amount) => "£" + amount.ToString("0.00", CultureInfo.InvariantCulture);

    public static string TinSvg(Tea tea) =>
        $"""<svg viewBox="0 0 60 80" xmlns="http://www.w3.org/2000/svg" aria-hidden="true" style="display:block;width:100%;height:100%"><rect x="6" y="4" width="48" height="10" rx="2" fill="{tea.Tin}" opacity=".75"/><rect x="8" y="12" width="44" height="64" rx="3" fill="{tea.Tin}"/><rect x="8" y="34" width="44" height="22" fill="{tea.Band}"/><circle cx="30" cy="45" r="6" fill="none" stroke="{tea.Tin}" stroke-width="1.5"/><path d="M30 39 q4 6 0 12 q-4 -6 0 -12" fill="{tea.Tin}"/></svg>""";

    // UK postcodes, loosely: "SW1A 1AA", "M1 1AE", "EH8 9YL". The space is optional.
    [GeneratedRegex(@"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$", RegexOptions.IgnoreCase)]
    private static partial Regex PostcodePattern();

    public static string? CheckPostcode(string? postcode) =>
        string.IsNullOrWhiteSpace(postcode) ? null : PostcodePattern().IsMatch(postcode.Trim()) ? null : "Enter a UK postcode, such as EH8 9YL.";

    public static string? CheckEmail(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null
        : System.Net.Mail.MailAddress.TryCreate(email.Trim(), out var parsed) && parsed.Address == email.Trim() ? null
        : "Enter an email address, such as you@example.org.";

    public static string? CheckPhone(string? phone) =>
        string.IsNullOrWhiteSpace(phone) || PaymentCard.Digits(phone).Length == 11 ? null : "Enter all 11 digits.";

    /// <summary>The Luhn checksum catches most mistyped card numbers before anything is sent.</summary>
    public static string? CheckCardNumber(string? number)
    {
        var digits = PaymentCard.Digits(number);
        if (digits.Length == 0) return null;
        if (digits.Length != 16) return "Enter all 16 digits.";
        var sum = 0;
        for (var index = 0; index < digits.Length; index++)
        {
            var digit = digits[digits.Length - 1 - index] - '0';
            if (index % 2 == 1) digit = digit * 2 > 9 ? digit * 2 - 9 : digit * 2;
            sum += digit;
        }
        return sum % 10 == 0 ? null : "This card number is not valid. Check for a typing mistake.";
    }

    public static string? CheckExpiry(string? expiry)
    {
        var digits = PaymentCard.Digits(expiry);
        if (digits.Length == 0) return null;
        if (digits.Length != 4) return "Use MM/YY.";
        var month = int.Parse(digits[..2], CultureInfo.InvariantCulture);
        var year = 2000 + int.Parse(digits[2..], CultureInfo.InvariantCulture);
        if (month is < 1 or > 12) return "The month must be 01 to 12.";
        return new DateTime(year, month, 1).AddMonths(1) <= Today ? "This card has expired." : null;
    }

    public static string CardBrand(string? number) => PaymentCard.Digits(number) switch
    {
        ['4', ..] => "Visa",
        ['5', '1' or '2' or '3' or '4' or '5', ..] => "Mastercard",
        ['3', '4' or '7', ..] => "Amex",
        _ => ""
    };

    /// <summary>Adds working days, skipping weekends: enough for a delivery estimate.</summary>
    public static DateTime AddWorkingDays(DateTime start, int days)
    {
        var date = start;
        while (days > 0)
        {
            date = date.AddDays(1);
            if (date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)) days--;
        }
        return date;
    }
}

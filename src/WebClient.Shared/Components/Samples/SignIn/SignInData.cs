using System.Net.Mail;
using MudBlazor;

namespace WebClient.Shared.Components.Samples.SignIn;

/// <summary>An account created on the other tab; it lives only in the page's memory.</summary>
public sealed record NewAccount(string Name, string Email, string Password);

/// <summary>One rule of the password policy and whether a candidate meets it.</summary>
public sealed record PasswordRule(string Text, bool Met);

/// <summary>How strong a new password is, with the rules behind the score.</summary>
public sealed record PasswordStrength(int Score, string Label, Color Color, IReadOnlyList<PasswordRule> Rules)
{
    public double Percent => Score * 25.0;
}

/// <summary>
/// The fictional observatory's sign-in rules. Everything is checked in memory: the sample never sends credentials,
/// stores them, or calls a server.
/// </summary>
public static class SignInData
{
    public const string DemoEmail = "ada@kestrel.example";
    public const string DemoPassword = "Meridian-42";
    public const string DemoName = "Ada Lindqvist";
    public const int MaxAttempts = 3;
    public const int LockoutSeconds = 20;

    public static IReadOnlyList<string> Roles { get; } = ["Visiting astronomer", "Student observer", "Telescope operator", "Outreach volunteer"];

    public static bool Matches(string? email, string? password) =>
        string.Equals(email?.Trim(), DemoEmail, StringComparison.OrdinalIgnoreCase) && password == DemoPassword;

    public static bool IsTaken(string? email) => string.Equals(email?.Trim(), DemoEmail, StringComparison.OrdinalIgnoreCase);

    public static string? ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "Enter your email address.";
        return MailAddress.TryCreate(email.Trim(), out var address) && address.Host.Contains('.') ? null : "Enter an email like name@example.org.";
    }

    public static PasswordStrength Strength(string? password)
    {
        password ??= "";
        PasswordRule[] rules =
        [
            new("At least 10 characters", password.Length >= 10),
            new("Upper and lower case letters", password.Any(char.IsUpper) && password.Any(char.IsLower)),
            new("A number", password.Any(char.IsDigit)),
            new("A symbol such as - or !", password.Any(c => !char.IsLetterOrDigit(c)))
        ];
        var score = rules.Count(rule => rule.Met);
        return score switch
        {
            4 => new(score, "Strong", Color.Success, rules),
            3 => new(score, "Good", Color.Info, rules),
            2 => new(score, "Fair", Color.Warning, rules),
            _ => new(score, password.Length == 0 ? "Not set" : "Weak", Color.Error, rules)
        };
    }

    public static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(part => char.ToUpperInvariant(part[0])));
}

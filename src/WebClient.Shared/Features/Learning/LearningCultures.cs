namespace WebClient.Shared.Features.Learning;

public static class LearningCultures
{
    public const string Default = "en-US";
    public static readonly string[] Supported = [Default, "pt-BR"];
    public static bool IsSupported(string? culture) => culture is not null && Supported.Contains(culture);
}

public static class LearningPolicies
{
    public const string Instructor = "Instructor";
    public static readonly string[] Personas = ["student", "instructor"];
}

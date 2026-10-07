namespace WebClient.Shared.Features.Learning;

/// <summary>
/// Describes the host running the shared labs. The Blazor Server host and the static WebAssembly demo
/// register different values, so pages can explain lifetimes honestly and render host-specific panels.
/// </summary>
public sealed record LearningHost
{
    public required string Name { get; init; }
    public required bool IsStaticDemo { get; init; }
    /// <summary>What a scoped service lives for, e.g. "circuit" or "browser tab".</summary>
    public required string ScopeLifetime { get; init; }
    /// <summary>Where component-owned work runs, e.g. "server" or "browser".</summary>
    public required string WorkLocation { get; init; }
    public required string NotebookStorage { get; init; }
    public required Type AuthPanel { get; init; }
    public required Type CulturePanel { get; init; }
    public required Type DiagnosticsPanel { get; init; }
    /// <summary>Host-specific notes shown inside a lab, keyed by lab slug.</summary>
    public IReadOnlyDictionary<string, string> LabNotes { get; init; } = new Dictionary<string, string>();
}

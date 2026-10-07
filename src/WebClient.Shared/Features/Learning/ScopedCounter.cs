namespace WebClient.Shared.Features.Learning;

/// <summary>A scoped service: one instance per Blazor Server circuit, or per browser tab in WebAssembly.</summary>
public sealed class ScopedCounter
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Count { get; set; }
}

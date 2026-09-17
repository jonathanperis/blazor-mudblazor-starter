namespace WebClient.Features.Learning;

public sealed class CircuitCounter
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Count { get; set; }
}

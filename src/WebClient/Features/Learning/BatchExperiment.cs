namespace WebClient.Features.Learning;

public static class BatchExperiment
{
    public static async Task RunAsync(int count, Func<int, Task> reportProgress, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, 100);
        for (var step = 1; step <= count; step++)
        {
            await Task.Delay(50, cancellationToken);
            await reportProgress(step * 100 / count);
        }
    }
}

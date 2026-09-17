using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebClient.Features.Notebook;

public sealed class NotebookHealthCheck(IDbContextFactory<NotebookDb> factory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        await db.Notes.Take(1).CountAsync(cancellationToken);
        return HealthCheckResult.Healthy("Notebook database is reachable.");
    }
}

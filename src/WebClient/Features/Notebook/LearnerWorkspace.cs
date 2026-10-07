using System.Security.Cryptography;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.DataProtection;

namespace WebClient.Features.Notebook;

public sealed class LearnerWorkspace(string id = "")
{
    public string Id { get; private set; } = id;
    internal const string CookieName = "learning.workspace";
    internal const string ItemName = "LearningWorkspace";

    /// <summary>
    /// Identifies the browser's workspace for page renders. Static assets, health probes and APIs never need it,
    /// so they stay cookie-free and cacheable. Each page render re-issues the cookie, so its 30-day lifetime
    /// slides while the learner keeps visiting instead of expiring and orphaning their notes.
    /// </summary>
    public static async Task EstablishAsync(HttpContext context, RequestDelegate next)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<ComponentTypeMetadata>() is null)
        {
            await next(context);
            return;
        }
        var protector = context.RequestServices.GetRequiredService<IDataProtectionProvider>().CreateProtector(CookieName);
        string? id = null;
        if (context.Request.Cookies.TryGetValue(CookieName, out var cookie))
        {
            try { id = protector.Unprotect(cookie); }
            catch (CryptographicException) { }
        }
        if (!Guid.TryParseExact(id, "N", out _)) id = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieName, protector.Protect(id), new CookieOptions
        {
            HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Lax,
            IsEssential = true, MaxAge = TimeSpan.FromDays(30)
        });
        context.Items[ItemName] = id;
        await next(context);
    }

    internal void Initialize(string workspaceId)
    {
        if (Id.Length > 0 && Id != workspaceId)
            throw new InvalidOperationException("A workspace cannot change inside an existing circuit. Reload the page to open a new workspace.");
        Id = workspaceId;
    }
}

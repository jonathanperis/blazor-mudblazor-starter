using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

namespace WebClient.Features.Identity;

public sealed record DemoAuthOptions(bool Enabled);

public static class DemoIdentity
{
    public const string InstructorPolicy = "Instructor";
    public static readonly string[] Cultures = ["en-US", "pt-BR"];

    public static void MapLearningIdentity(this WebApplication app)
    {
        app.MapPost("/auth/demo", async (HttpContext context, IAntiforgery antiforgery, DemoAuthOptions options) =>
        {
            if (!options.Enabled) return Results.NotFound();
            if (!await ValidForm(context, antiforgery)) return Results.BadRequest();
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var persona = form["persona"].ToString();
            if (persona is not ("student" or "instructor")) return Results.BadRequest();
            var identity = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, $"demo:{persona}"),
                new Claim(ClaimTypes.Name, $"Demo {persona}"),
                new Claim(ClaimTypes.Role, persona)
            ], CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return Results.LocalRedirect("/labs/auth");
        });

        app.MapPost("/auth/logout", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            if (!await ValidForm(context, antiforgery)) return Results.BadRequest();
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.LocalRedirect("/labs/auth");
        });

        app.MapGet("/api/instructor", (ClaimsPrincipal user) => Results.Ok(new { message = "Instructor policy passed on the server.", name = user.Identity!.Name }))
            .RequireAuthorization(InstructorPolicy);

        app.MapPost("/culture", async (HttpContext context, IAntiforgery antiforgery) =>
        {
            if (!await ValidForm(context, antiforgery)) return Results.BadRequest();
            var form = await context.Request.ReadFormAsync(context.RequestAborted);
            var culture = form["culture"].ToString();
            if (!Cultures.Contains(culture)) return Results.BadRequest();
            context.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Lax, IsEssential = true, MaxAge = TimeSpan.FromDays(30) });
            return Results.LocalRedirect("/labs/localization");
        });
    }

    private static async Task<bool> ValidForm(HttpContext context, IAntiforgery antiforgery)
    {
        if (!context.Request.HasFormContentType) return false;
        try { await antiforgery.ValidateRequestAsync(context); return true; }
        catch (AntiforgeryValidationException) { return false; }
    }
}

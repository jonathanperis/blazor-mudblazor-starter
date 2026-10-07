using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using MudBlazor.Translations;
using WebClient.Shared.Features.Forecasts;

namespace WebClient.Shared.Features.Learning;

public static class LearningServices
{
    /// <summary>Registers the services every host needs to render the shared labs.</summary>
    public static IServiceCollection AddLearningLabs(this IServiceCollection services, LearningHost host)
    {
        services.AddMudServices(options => options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight);
        services.AddMudTranslations();
        services.AddLocalization();
        services.AddSingleton(host);
        services.AddSingleton<ForecastCatalog>();
        // Scoped: one per circuit in Blazor Server, one per browser tab in WebAssembly.
        services.AddScoped<ScopedCounter>();
        services.AddScoped<UiPreferences>();
        return services;
    }
}

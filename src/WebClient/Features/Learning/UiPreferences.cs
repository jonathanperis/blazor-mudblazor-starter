using Microsoft.JSInterop;

namespace WebClient.Features.Learning;

public sealed record PreferenceSnapshot(bool IsDarkMode, bool DrawerOpen, bool Available);

public sealed class UiPreferences(IJSRuntime js)
{
    public bool IsDarkMode { get; private set; }
    public bool DrawerOpen { get; private set; } = true;
    public bool Available { get; private set; } = true;

    public async Task LoadAsync()
    {
        var snapshot = await js.InvokeAsync<PreferenceSnapshot>("learningPreferences.read");
        IsDarkMode = snapshot.IsDarkMode;
        DrawerOpen = snapshot.DrawerOpen;
        Available = snapshot.Available;
    }

    public async Task SetThemeAsync(bool value)
    {
        IsDarkMode = value;
        Available = await js.InvokeAsync<bool>("learningPreferences.write", "isDarkMode", value);
    }

    public async Task SetDrawerAsync(bool value)
    {
        DrawerOpen = value;
        Available = await js.InvokeAsync<bool>("learningPreferences.write", "drawerOpen", value);
    }
}

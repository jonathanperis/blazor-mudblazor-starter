using Microsoft.JSInterop;

namespace WebClient.Shared.Features.Learning;

public sealed record PreferenceSnapshot(bool IsDarkMode, bool DrawerOpen, bool Available);

/// <summary>
/// Theme and drawer preferences stored in browser storage. Storage or interop failures degrade to
/// in-memory preferences; they must never stop the shell from rendering.
/// </summary>
public sealed class UiPreferences(IJSRuntime js)
{
    private bool _themeChanged;
    private bool _drawerChanged;

    public bool IsDarkMode { get; private set; }
    public bool DrawerOpen { get; private set; } = true;
    public bool Available { get; private set; } = true;

    public async Task LoadAsync()
    {
        PreferenceSnapshot snapshot;
        try { snapshot = await js.InvokeAsync<PreferenceSnapshot>("learningPreferences.read"); }
        catch (JSException) { Available = false; return; }
        // A user may toggle before the stored values arrive; the newer choice wins.
        if (!_themeChanged) IsDarkMode = snapshot.IsDarkMode;
        if (!_drawerChanged) DrawerOpen = snapshot.DrawerOpen;
        Available = snapshot.Available;
    }

    public async Task SetThemeAsync(bool value)
    {
        IsDarkMode = value;
        _themeChanged = true;
        await WriteAsync("isDarkMode", value);
    }

    /// <summary>Persists an explicit drawer choice made with the menu button.</summary>
    public async Task SetDrawerAsync(bool value)
    {
        DrawerOpen = value;
        _drawerChanged = true;
        await WriteAsync("drawerOpen", value);
    }

    /// <summary>
    /// Mirrors a drawer state the layout changed by itself, such as a responsive drawer closing after
    /// navigation on a small screen. Screen size is derived, so this is not persisted.
    /// </summary>
    public void ObserveDrawer(bool value) => DrawerOpen = value;

    private async Task WriteAsync(string key, bool value)
    {
        try { Available = await js.InvokeAsync<bool>("learningPreferences.write", key, value); }
        catch (JSException) { Available = false; }
    }
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using WebClient.Shared.Features.Notebook;

namespace WebClient.Prerender;

/// <summary>The address of the page being rendered. The renderer never navigates.</summary>
internal sealed class StaticNavigationManager : NavigationManager
{
    public void Start(string baseUri, string uri) => Initialize(baseUri, uri);

    protected override void NavigateToCore(string uri, NavigationOptions options) =>
        throw new InvalidOperationException($"A prerendered page tried to navigate to {uri} while rendering.");
}

/// <summary>
/// There is no browser at build time. Components call JavaScript after rendering, which a static render never
/// reaches; a call during rendering is reported as a disconnected browser, which components already handle.
/// </summary>
internal sealed class StaticJSRuntime : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        ValueTask.FromException<TValue>(new JSDisconnectedException($"No browser while prerendering ({identifier})."));

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        InvokeAsync<TValue>(identifier, args);
}

/// <summary>Collects exceptions that error boundaries caught, so a page that renders its error state fails the build.</summary>
internal sealed class CollectingErrorBoundaryLogger : IErrorBoundaryLogger
{
    public List<Exception> Errors { get; } = [];

    public ValueTask LogErrorAsync(Exception exception)
    {
        Errors.Add(exception);
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// Notes live in each visitor's browser, so the build cannot list them. Listing waits until the page is disposed;
/// the prerendered notebook shows its loading state, and the browser fills it in.
/// </summary>
internal sealed class PendingNotebookStore : INotebookStore
{
    /// <summary>Whether a page asked for the notes, so its prerender may stop at the loading state.</summary>
    public bool Waiting { get; private set; }

    public async Task<List<NoteSnapshot>> ListAsync(CancellationToken cancellationToken = default)
    {
        Waiting = true;
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return [];
    }

    public Task SaveAsync(NoteDraft draft, NoteSnapshot? original = null, CancellationToken cancellationToken = default) => Unavailable();
    public Task DeleteAsync(NoteSnapshot original, CancellationToken cancellationToken = default) => Unavailable();
    public Task ResetAsync(CancellationToken cancellationToken = default) => Unavailable();

    private static Task Unavailable() => Task.FromException(new InvalidOperationException("Prerendering cannot change a visitor's notebook."));
}

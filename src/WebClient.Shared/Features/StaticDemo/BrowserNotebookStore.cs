using System.ComponentModel.DataAnnotations;
using Microsoft.JSInterop;
using WebClient.Shared.Features.Notebook;

namespace WebClient.Shared.Features.StaticDemo;

/// <summary>
/// The static demo's notebook: browser localStorage instead of SQLite, with the same contract. Versions are
/// compared and swapped inside one synchronous JavaScript call, so two tabs still produce conflicts.
/// </summary>
/// <summary>A note as stored in localStorage; GUIDs are strings in JSON.</summary>
internal sealed record BrowserNote(string Id, string Title, string Text, string Version);

public sealed class BrowserNotebookStore(IJSRuntime js) : INotebookStore
{
    public async Task<List<NoteSnapshot>> ListAsync(CancellationToken cancellationToken = default)
    {
        var notes = await CallAsync<BrowserNote[]?>("learningNotebook.list", cancellationToken) ?? [];
        return notes
            .Select(note => Guid.TryParse(note.Id, out var id) && Guid.TryParse(note.Version, out var version)
                ? new NoteSnapshot(id, note.Title ?? "", note.Text ?? "", version) : null)
            .OfType<NoteSnapshot>()
            .OrderBy(note => note.Title, StringComparer.Ordinal)
            .ToList();
    }

    public async Task SaveAsync(NoteDraft draft, NoteSnapshot? original = null, CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(draft, new ValidationContext(draft), true);
        var note = new BrowserNote((original?.Id ?? Guid.NewGuid()).ToString(), draft.Title.Trim(), draft.Text, Guid.NewGuid().ToString());
        var outcome = original is null
            ? await CallAsync<string>("learningNotebook.insert", cancellationToken, note, INotebookStore.MaxNotes)
            : await CallAsync<string>("learningNotebook.update", cancellationToken, note, original.Version.ToString());
        ThrowFor(outcome);
    }

    public async Task DeleteAsync(NoteSnapshot original, CancellationToken cancellationToken = default) =>
        ThrowFor(await CallAsync<string>("learningNotebook.remove", cancellationToken, original.Id.ToString(), original.Version.ToString()));

    public Task ResetAsync(CancellationToken cancellationToken = default) => CallAsync<object?>("learningNotebook.reset", cancellationToken);

    private static void ThrowFor(string outcome)
    {
        if (outcome == "conflict") throw new NotebookConflictException("The note was changed or removed after this draft was opened.");
        if (outcome == "limit") throw new NotebookLimitException();
    }

    private async Task<T> CallAsync<T>(string identifier, CancellationToken cancellationToken, params object?[] args)
    {
        try { return await js.InvokeAsync<T>(identifier, cancellationToken, args); }
        catch (JSException error) { throw new NotebookUnavailableException("Browser storage is unavailable. Allow site data for this page to keep notes.", error); }
    }
}

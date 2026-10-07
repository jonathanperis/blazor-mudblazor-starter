using System.ComponentModel.DataAnnotations;

namespace WebClient.Shared.Features.Notebook;

public sealed class NoteDraft
{
    [Required, StringLength(120)] public string Title { get; set; } = "";
    [StringLength(4000)] public string Text { get; set; } = "";
}

public sealed record NoteSnapshot(Guid Id, string Title, string Text, Guid Version);

/// <summary>
/// Learner-owned notes. Every write carries the version it was based on, so a stale draft is rejected
/// instead of silently overwriting a newer change.
/// </summary>
public interface INotebookStore
{
    /// <summary>Notes per workspace. Each note is bounded, so the workspace is bounded too.</summary>
    const int MaxNotes = 50;

    Task<List<NoteSnapshot>> ListAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(NoteDraft draft, NoteSnapshot? original = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(NoteSnapshot original, CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}

/// <summary>The note changed or disappeared after the draft's snapshot was taken.</summary>
public sealed class NotebookConflictException(string message, Exception? inner = null) : Exception(message, inner);

/// <summary>The workspace already holds <see cref="INotebookStore.MaxNotes"/> notes.</summary>
public sealed class NotebookLimitException() : Exception($"A workspace holds at most {INotebookStore.MaxNotes} notes. Delete one or reset the notebook.");

/// <summary>The storage behind the notebook cannot be reached, for example disabled browser storage.</summary>
public sealed class NotebookUnavailableException(string message, Exception? inner = null) : Exception(message, inner);

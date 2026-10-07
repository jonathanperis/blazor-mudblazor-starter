using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using WebClient.Shared.Features.Notebook;

namespace WebClient.Features.Notebook;

/// <summary>
/// The server notebook: SQLite through EF Core. Each operation creates a short-lived DbContext, filters by the
/// current workspace, and uses the version column as an optimistic concurrency token.
/// </summary>
public sealed class NotebookService(IDbContextFactory<NotebookDb> factory, LearnerWorkspace workspace) : INotebookStore
{
    public async Task<List<NoteSnapshot>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        return await db.Notes.AsNoTracking().Where(n => n.Workspace == workspace.Id).OrderBy(n => n.Title)
            .Select(n => new NoteSnapshot(n.Id, n.Title, n.Text, n.Version)).ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(NoteDraft draft, NoteSnapshot? original = null, CancellationToken cancellationToken = default)
    {
        Validator.ValidateObject(draft, new ValidationContext(draft), true);
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        Note note;
        if (original is null)
        {
            // A soft bound: two simultaneous inserts can pass the check together, which is acceptable here.
            if (await db.Notes.CountAsync(n => n.Workspace == workspace.Id, cancellationToken) >= INotebookStore.MaxNotes)
                throw new NotebookLimitException();
            note = new Note { Id = Guid.NewGuid(), Workspace = workspace.Id };
            db.Notes.Add(note);
        }
        else
        {
            note = await FindAsync(db, original, cancellationToken);
        }
        note.Title = draft.Title.Trim();
        note.Text = draft.Text;
        note.Version = Guid.NewGuid();
        await SaveChangesAsync(db, cancellationToken);
    }

    public async Task DeleteAsync(NoteSnapshot original, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        db.Notes.Remove(await FindAsync(db, original, cancellationToken));
        await SaveChangesAsync(db, cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        await db.Notes.Where(n => n.Workspace == workspace.Id).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task<Note> FindAsync(NotebookDb db, NoteSnapshot original, CancellationToken cancellationToken)
    {
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == original.Id && n.Workspace == workspace.Id, cancellationToken)
            ?? throw new NotebookConflictException("The note was removed or is no longer available.");
        // Compare against the version the draft started from, not the version just read.
        db.Entry(note).Property(n => n.Version).OriginalValue = original.Version;
        return note;
    }

    private static async Task SaveChangesAsync(NotebookDb db, CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException error) { throw new NotebookConflictException("The note was changed after this draft was opened.", error); }
    }
}

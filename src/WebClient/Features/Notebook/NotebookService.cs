using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebClient.Features.Notebook;

public sealed class NotebookService(IDbContextFactory<NotebookDb> factory, LearnerWorkspace workspace)
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
            note = new Note { Id = Guid.NewGuid(), Workspace = workspace.Id };
            db.Notes.Add(note);
        }
        else
        {
            note = await db.Notes.SingleOrDefaultAsync(n => n.Id == original.Id && n.Workspace == workspace.Id, cancellationToken)
                ?? throw new DbUpdateConcurrencyException("The note was removed or is no longer available.");
            db.Entry(note).Property(n => n.Version).OriginalValue = original.Version;
        }
        note.Title = draft.Title.Trim();
        note.Text = draft.Text;
        note.Version = Guid.NewGuid();
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(NoteSnapshot original, CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        var note = await db.Notes.SingleOrDefaultAsync(n => n.Id == original.Id && n.Workspace == workspace.Id, cancellationToken)
            ?? throw new DbUpdateConcurrencyException("The note was removed or is no longer available.");
        db.Entry(note).Property(n => n.Version).OriginalValue = original.Version;
        db.Notes.Remove(note);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await factory.CreateDbContextAsync(cancellationToken);
        await db.Notes.Where(n => n.Workspace == workspace.Id).ExecuteDeleteAsync(cancellationToken);
    }
}

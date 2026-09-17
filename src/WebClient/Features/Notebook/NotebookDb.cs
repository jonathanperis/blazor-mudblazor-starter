using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebClient.Features.Notebook;

public sealed class NoteDraft
{
    [Required, StringLength(120)] public string Title { get; set; } = "";
    [StringLength(4000)] public string Text { get; set; } = "";
}

public sealed record NoteSnapshot(Guid Id, string Title, string Text, Guid Version);

public sealed class Note
{
    public Guid Id { get; set; }
    public string Workspace { get; set; } = "";
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public Guid Version { get; set; }
}

public sealed class NotebookDb(DbContextOptions<NotebookDb> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Note>(note =>
        {
            note.HasKey(n => n.Id);
            note.HasIndex(n => n.Workspace);
            note.Property(n => n.Workspace).HasMaxLength(32).IsRequired();
            note.Property(n => n.Title).HasMaxLength(120).IsRequired();
            note.Property(n => n.Text).HasMaxLength(4000).IsRequired();
            note.Property(n => n.Version).IsConcurrencyToken();
        });
    }
}

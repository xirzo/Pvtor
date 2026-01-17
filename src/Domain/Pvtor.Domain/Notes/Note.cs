using System;

namespace Pvtor.Domain.Notes;

public sealed record Note
{
    public Note(NoteId noteId, string content, DateTime creationDate, DateTime updateDate)
    {
        NoteId = noteId;
        Content = content;
        CreationDate = creationDate;
        UpdateDate = updateDate;
    }

    public NoteId NoteId { get; }

    public string Content { get; }

    public DateTime CreationDate { get; }

    public DateTime UpdateDate { get; }
}
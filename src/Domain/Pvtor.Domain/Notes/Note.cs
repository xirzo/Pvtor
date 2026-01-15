using System;

namespace Pvtor.Domain.Notes;

public sealed record Note
{
    public Note(NoteId noteId, string content, DateTime creationDate)
    {
        NoteId = noteId;
        Content = content;
        CreationDate = creationDate;
    }

    public NoteId NoteId { get; }

    public string Content { get; }

    public DateTime CreationDate { get; }
}
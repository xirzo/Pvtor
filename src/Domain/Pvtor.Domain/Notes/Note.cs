using Pvtor.Domain.Notes.Namespaces;
using System;

namespace Pvtor.Domain.Notes;

public sealed record Note
{
    public Note(
        NoteId noteId,
        string content,
        DateTime creationDate,
        DateTime updateDate,
        NoteNamespaceId? noteNamespaceId)
    {
        NoteId = noteId;
        Content = content;
        CreationDate = creationDate;
        UpdateDate = updateDate;
        NoteNamespaceId = noteNamespaceId;
    }

    public NoteId NoteId { get; }

    public string Content { get; init; }

    public DateTime CreationDate { get; }

    public DateTime UpdateDate { get; }

    public NoteNamespaceId? NoteNamespaceId { get; }
}
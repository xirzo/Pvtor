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
        NoteNamespaceId? noteNamespaceId,
        bool isHidden)
    {
        NoteId = noteId;
        Content = content;
        CreationDate = creationDate;
        UpdateDate = updateDate;
        NoteNamespaceId = noteNamespaceId;
        IsHidden = isHidden;
    }

    public NoteId NoteId { get; }

    public string Content { get; init; }

    public DateTime CreationDate { get; }

    public DateTime UpdateDate { get; }

    public NoteNamespaceId? NoteNamespaceId { get; }

    public bool IsHidden { get; init; }
}
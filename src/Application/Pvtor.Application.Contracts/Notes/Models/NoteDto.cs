using System;

namespace Pvtor.Application.Contracts.Notes.Models;

public sealed record NoteDto(
    long NoteId,
    string? Name,
    string Content,
    DateTime CreationDate,
    DateTime UpdateDate,
    long? NoteNamespaceId,
    bool IsHidden);
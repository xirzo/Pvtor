using System;

namespace Pvtor.Application.Contracts.Notes.Models;

public sealed record NoteDto(long NoteId, string Content, DateTime CreationDate, long? NoteNamespaceId, bool IsHidden);
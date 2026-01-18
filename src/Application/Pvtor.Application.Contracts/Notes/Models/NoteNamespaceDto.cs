using System;

namespace Pvtor.Application.Contracts.Notes.Models;

public record class NoteNamespaceDto(long NoteNamespaceId, string Name, DateTime CreationDate);
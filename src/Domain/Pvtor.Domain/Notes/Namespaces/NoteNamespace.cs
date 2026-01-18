using System;

namespace Pvtor.Domain.Notes.Namespaces;

public sealed record NoteNamespace(NoteNamespaceId NoteNamespaceId, string Name, DateTime CreationDate);
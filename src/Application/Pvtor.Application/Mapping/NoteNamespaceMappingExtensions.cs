using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Domain.Notes.Namespaces;

namespace Pvtor.Application.Mapping;

public static class NoteNamespaceMappingExtensions
{
    public static NoteNamespaceDto MapToDto(this NoteNamespace note)
    {
        return new NoteNamespaceDto(note.NoteNamespaceId.Value, note.Name, note.CreationDate);
    }
}
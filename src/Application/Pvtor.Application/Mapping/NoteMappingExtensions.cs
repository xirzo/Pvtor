using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Domain.Notes;

namespace Pvtor.Application.Mapping;

public static class NoteMappingExtensions
{
    public static NoteDto MapToDto(this Note note)
    {
        return new NoteDto(note.NoteId.Value, note.Name, note.Content, note.CreationDate, note.NoteNamespaceId?.Value, note.IsHidden);
    }
}
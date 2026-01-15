using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes;
using System;

namespace Pvtor.Application.Services;

internal sealed class NoteService : INoteService
{
    private readonly IPersistanceContext _context;

    public NoteService(IPersistanceContext context)
    {
        _context = context;
    }

    public CreateNote.Response CreateNote(CreateNote.Request request)
    {
        try
        {
            Note note = _context.NoteRepository.Add(new Note(NoteId.Default, request.Content, DateTime.UtcNow));
            return new CreateNote.Response.Success(note.MapToDto());
        }
        catch (Exception ex)
        {
            return new CreateNote.Response.PersistenceFailure(ex.Message);
        }
    }
}
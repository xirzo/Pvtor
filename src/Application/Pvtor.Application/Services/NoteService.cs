using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Services;

internal sealed class NoteService : INoteService
{
    private readonly IPersistanceContext _context;

    public NoteService(IPersistanceContext context)
    {
        _context = context;
    }

    public async Task<CreateNote.Response> CreateNoteAsync(
        CreateNote.Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Note note = await _context.NoteRepository.AddAsync(
                new Note(NoteId.Default, request.Content, DateTime.UtcNow, DateTime.UtcNow),
                cancellationToken);
            return new CreateNote.Response.Success(note.MapToDto());
        }
        catch (Exception ex)
        {
            return new CreateNote.Response.PersistenceFailure(ex.Message);
        }
    }
}
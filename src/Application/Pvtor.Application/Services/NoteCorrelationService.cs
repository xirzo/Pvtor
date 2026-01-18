using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Services;

public class NoteCorrelationService : INoteCorrelationService
{
    private readonly IPersistanceContext _context;

    public NoteCorrelationService(IPersistanceContext context)
    {
        _context = context;
    }

    public async Task<RecordCorrelation.Response> RecordCorrelationAsync(
        RecordCorrelation.Request request,
        CancellationToken cancellationToken = default)
    {
        var noteCorrelation =
            new NoteCorrelation(
                new NoteCorrelationId(new NoteSourceId(request.NoteSourceId), new NoteId(request.NoteId)),
                DateTime.UtcNow);

        try
        {
            await _context.NoteCorrelationRepository.AddAsync(noteCorrelation, cancellationToken);
            return new RecordCorrelation.Response.Success();
        }
        catch (Exception ex)
        {
            return new RecordCorrelation.Response.PersistenceFailure(ex.Message);
        }
    }

    public async Task<IEnumerable<NoteCorrelationDto>> FindBySourceIdAsync(string noteSourceId)
    {
        var query =
            NoteCorrelationQuery.Build(builder => builder.WithNoteSourceId(new NoteSourceId(noteSourceId)));

        IEnumerable<NoteCorrelation> noteCorrelation = await _context.NoteCorrelationRepository
            .QueryAsync(query);

        return noteCorrelation.Select(x => x.MapToDto());
    }

    public async Task<IEnumerable<NoteCorrelationDto>> FindByNoteIdAsync(long noteId)
    {
        var query =
            NoteCorrelationQuery.Build(builder => builder.WithNoteId(new NoteId(noteId)));

        IEnumerable<NoteCorrelation> noteCorrelation = await _context.NoteCorrelationRepository
            .QueryAsync(query);

        return noteCorrelation.Select(x => x.MapToDto());
    }
}
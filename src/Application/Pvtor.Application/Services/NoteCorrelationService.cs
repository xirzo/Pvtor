using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes;
using System;
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

    public async Task RecordCorrelationAsync(
        long noteId,
        string noteSourceId,
        CancellationToken cancellationToken = default)
    {
        var noteCorrelation =
            new NoteCorrelation(
                new NoteCorrelationId(new NoteSourceId(noteSourceId), new NoteId(noteId)),
                DateTime.UtcNow);

        await _context.NoteCorrelationRepository.AddAsync(noteCorrelation, cancellationToken);
    }

    public async Task<NoteCorrelationDto?> FindBySourceIdAsync(string noteSourceId)
    {
        var query =
            NoteCorrelationQuery.Build(builder => builder.WithNoteSourceId(new NoteSourceId(noteSourceId)));

        NoteCorrelation? noteCorrelation = (await _context.NoteCorrelationRepository
                .QueryAsync(query))
            .SingleOrDefault();

        if (noteCorrelation is null)
        {
            return null;
        }

        return noteCorrelation.MapToDto();
    }
}
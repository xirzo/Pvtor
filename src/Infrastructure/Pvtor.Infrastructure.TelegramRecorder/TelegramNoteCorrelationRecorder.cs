using Pvtor.Application.Abstractions;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.TelegramRecorder;

public class TelegramNoteCorrelationRecorder : INoteCorrelationRecorder
{
    private readonly INoteCorrelationRepository _correlationRepository;

    public TelegramNoteCorrelationRecorder(INoteCorrelationRepository correlationRepository)
    {
        _correlationRepository = correlationRepository;
    }

    public async Task RecordCorrelationAsync(
        NoteId noteId,
        NoteSourceId noteSourceId,
        CancellationToken cancellationToken = default)
    {
        var noteCorrelation = new NoteCorrelation(new NoteCorrelationId(noteSourceId, noteId), DateTime.UtcNow);

        await _correlationRepository.AddAsync(noteCorrelation, cancellationToken);
    }
}
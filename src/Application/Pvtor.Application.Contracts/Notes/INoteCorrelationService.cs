using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteCorrelationService
{
    Task<RecordCorrelation.Response> RecordCorrelationAsync(
        RecordCorrelation.Request request,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NoteCorrelationDto>> FindBySourceIdAsync(string noteSourceId);

    Task<IEnumerable<NoteCorrelationDto>> FindByNoteIdAsync(long noteId);
}
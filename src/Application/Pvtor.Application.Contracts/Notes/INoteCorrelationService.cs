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

    Task<DeleteCorrelation.Response> DeleteAsync(
        DeleteCorrelation.Request request,
        CancellationToken cancellationToken = default);

    // TODO: replace by query
    Task<IEnumerable<NoteCorrelationDto>> FindBySourceIdAsync(string noteSourceId);

    // TODO: replace by query
    Task<IEnumerable<NoteCorrelationDto>> FindByNoteIdAsync(long noteId);
}
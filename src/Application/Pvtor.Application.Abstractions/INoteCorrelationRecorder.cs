using Pvtor.Domain.Notes;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions;

public interface INoteCorrelationRecorder
{
    Task RecordCorrelationAsync(NoteId noteId, NoteSourceId noteSourceId, CancellationToken cancellationToken = default);
}
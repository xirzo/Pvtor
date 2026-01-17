using Pvtor.Application.Contracts.Notes.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteCorrelationService
{
    Task RecordCorrelationAsync(long noteId, string noteSourceId, CancellationToken cancellationToken = default);

    Task<NoteCorrelationDto?> FindBySourceIdAsync(string noteSourceId);
}
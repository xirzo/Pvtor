using Pvtor.Domain.Notes;

namespace Pvtor.Application.Abstractions;

public interface INoteCorrelationRecorder
{
    void RecordCorrelation(NoteId noteId, NoteSourceId noteSourceId);
}
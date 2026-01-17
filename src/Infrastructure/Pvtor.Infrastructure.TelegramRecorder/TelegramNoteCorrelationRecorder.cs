using Pvtor.Application.Abstractions;
using Pvtor.Domain.Notes;

namespace Pvtor.Infrastructure.TelegramRecorder;

public class TelegramNoteCorrelationRecorder : INoteCorrelationRecorder
{
    public void RecordCorrelation(NoteId noteId, NoteSourceId noteSourceId)
    {
        throw new System.NotImplementedException();
    }
}
namespace Pvtor.Domain.Notes;

public readonly record struct NoteCorrelationId(NoteSourceId NoteSourceId, NoteId NoteId)
{
    public static readonly NoteCorrelationId Default = new(NoteSourceId.Default, NoteId.Default);
}
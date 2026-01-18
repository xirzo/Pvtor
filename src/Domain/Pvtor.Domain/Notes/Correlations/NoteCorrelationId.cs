using Pvtor.Domain.Notes.Channels;

namespace Pvtor.Domain.Notes.Correlations;

public readonly record struct NoteCorrelationId(NoteSourceId NoteSourceId, NoteChannelId NoteChannelId)
{
    public static readonly NoteCorrelationId Default = new(NoteSourceId.Default, NoteChannelId.Default);
}
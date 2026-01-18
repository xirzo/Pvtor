namespace Pvtor.Domain.Notes.Channels;

public readonly record struct NoteChannelId(long Value)
{
    public static readonly NoteChannelId Default = new(0);
}
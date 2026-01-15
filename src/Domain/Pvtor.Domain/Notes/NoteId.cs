namespace Pvtor.Domain.Notes;

public readonly record struct NoteId(long Value)
{
    public static readonly NoteId Default = new NoteId(0);
}
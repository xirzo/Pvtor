namespace Pvtor.Domain.Notes;

public readonly record struct NoteSourceId(string Value)
{
    public static readonly NoteSourceId Default = new NoteSourceId(string.Empty);
}

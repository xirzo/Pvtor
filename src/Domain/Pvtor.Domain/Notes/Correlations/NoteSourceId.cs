namespace Pvtor.Domain.Notes.Correlations;

public readonly record struct NoteSourceId(string Value)
{
    public static readonly NoteSourceId Default = new NoteSourceId(string.Empty);
}

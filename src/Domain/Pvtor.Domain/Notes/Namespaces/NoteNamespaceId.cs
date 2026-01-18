namespace Pvtor.Domain.Notes.Namespaces;

public readonly record struct NoteNamespaceId(long Value)
{
    public static readonly NoteNamespaceId Default = new(0);
}
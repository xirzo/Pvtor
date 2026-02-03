namespace Pvtor.Presentation.Http.Parameters;

public class QueryNotesParameters
{
    public long[] NoteIds { get; set; } = [];

    public long[] NamespaceIds { get; set; } = [];

    public bool OnlyNonHidden { get; set; }
}
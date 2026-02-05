namespace Pvtor.Presentation.Http.Parameters;

public class QueryNotesParameters
{
    public long[] NoteIds { get; set; } = [];

    /// <summary>
    /// Value 0 is equal to null
    /// </summary>
    public long[] NamespaceIds { get; set; } = [];

    public bool OnlyNonHidden { get; set; }

    public string Content { get; set; } = string.Empty;
}
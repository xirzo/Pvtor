namespace Pvtor.Presentation.Http.Parameters;

public class QueryNamespacesParameters
{
    /// <summary>
    /// Value 0 cannot be searched, as null namespace is not recorded
    /// </summary>
    public long[] NamespaceIds { get; set; } = [];
}
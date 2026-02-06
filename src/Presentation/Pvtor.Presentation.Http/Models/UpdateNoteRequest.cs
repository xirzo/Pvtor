using System.Diagnostics.CodeAnalysis;

namespace Pvtor.Presentation.Http.Models;

public class UpdateNoteRequest
{
    [NotNull]
    public string? Name { get; set; }

    [NotNull]
    public string? Content { get; set; }
}
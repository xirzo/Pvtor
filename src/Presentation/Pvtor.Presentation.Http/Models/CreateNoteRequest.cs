using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Pvtor.Presentation.Http.Models;

public sealed class CreateNoteRequest
{
    [NotNull]
    public string? Name { get; set; }

    [Required]
    public long? NamespaceId { get; set; }

    [NotNull]
    [Required]
    public string? Content { get; set; }
}
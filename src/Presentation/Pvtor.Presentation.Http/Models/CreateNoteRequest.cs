using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Pvtor.Presentation.Http.Models;

public sealed class CreateNoteRequest
{
    [NotNull]
    [Required]
    public string? Content { get; set; }
}
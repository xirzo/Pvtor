using System;

namespace Pvtor.Domain.Notes;

public sealed record NoteCorrelation(NoteCorrelationId NoteCorrelationId, string SourceName, DateTime CreationDate);
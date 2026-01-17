using System;

namespace Pvtor.Domain.Notes;

public sealed record NoteCorrelation(NoteCorrelationId NoteCorrelationId, DateTime CreationDate);
using System;

namespace Pvtor.Domain.Notes.Correlations;

public sealed record NoteCorrelation(NoteCorrelationId NoteCorrelationId, NoteId NoteId, DateTime CreationDate);
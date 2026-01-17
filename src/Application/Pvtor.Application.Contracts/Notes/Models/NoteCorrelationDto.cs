using System;

namespace Pvtor.Application.Contracts.Notes.Models;

public sealed record NoteCorrelationDto(string NoteSourceId, long NoteId, DateTime CreationDate);
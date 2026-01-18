using System;

namespace Pvtor.Application.Contracts.Notes.Models;

public record NoteChannelDto(long NoteChannelId, string NoteSourceChannelId, DateTime CreationDate);
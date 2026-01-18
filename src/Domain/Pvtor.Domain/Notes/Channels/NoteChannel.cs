using Pvtor.Domain.Notes.Namespaces;
using System;

namespace Pvtor.Domain.Notes.Channels;

public record NoteChannel(
    NoteChannelId NoteChannelId,
    string NoteSourceChannelId,
    DateTime CreationDate,
    NoteNamespaceId? NoteNamespaceId);
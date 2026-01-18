using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Domain.Notes.Channels;

namespace Pvtor.Application.Mapping;

public static class NoteChannelMappingExtensions
{
    public static NoteChannelDto MapToDto(this NoteChannel noteChannel)
    {
        return new NoteChannelDto(
            noteChannel.NoteChannelId.Value,
            noteChannel.NoteSourceChannelId,
            noteChannel.CreationDate,
            noteChannel.NoteNamespaceId?.Value);
    }
}
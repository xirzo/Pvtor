using Pvtor.Domain.Notes.Channels;
using SourceKit.Generators.Builder.Annotations;

namespace Pvtor.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public sealed partial record NoteChannelQuery(
    NoteChannelId[] Ids,
    string[] NoteSourceChannelIds);
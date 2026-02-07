using Pvtor.Domain.Notes;
using Pvtor.Domain.Notes.Channels;
using Pvtor.Domain.Notes.Namespaces;
using SourceKit.Generators.Builder.Annotations;

namespace Pvtor.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public sealed partial record NoteQuery(
    NoteId[] Ids,
    NoteNamespaceId[] NoteNamespaceIds,
    NoteChannelId[] NoteChannelIds,
    string Content,

    // TODO: maybe replace with array of strings
    string SortOrder,
    bool UseNullNamespace,
    bool OnlyNonHidden);
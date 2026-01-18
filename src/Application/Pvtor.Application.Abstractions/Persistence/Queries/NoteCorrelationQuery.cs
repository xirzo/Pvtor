using Pvtor.Domain.Notes;
using Pvtor.Domain.Notes.Correlations;
using SourceKit.Generators.Builder.Annotations;

namespace Pvtor.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public sealed partial record NoteCorrelationQuery(
    NoteCorrelationId[] Ids,
    NoteSourceId[] NoteSourceIds,
    NoteId[] NoteIds);
using Pvtor.Domain.Notes;
using SourceKit.Generators.Builder.Annotations;

namespace Pvtor.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public sealed partial record NoteCorrelationQuery(NoteCorrelationId[] Ids, NoteSourceId[] SourceIds);
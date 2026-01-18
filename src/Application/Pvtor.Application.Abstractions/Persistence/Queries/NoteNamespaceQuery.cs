using Pvtor.Domain.Notes.Namespaces;
using SourceKit.Generators.Builder.Annotations;

namespace Pvtor.Application.Abstractions.Persistence.Queries;

[GenerateBuilder]
public sealed partial record NoteNamespaceQuery(NoteNamespaceId[] NoteNamespaceIds, string[] Names);
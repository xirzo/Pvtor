using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes.Namespaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteNamespaceRepository
{
    Task<NoteNamespace> AddAsync(NoteNamespace noteNamespace);

    Task<IEnumerable<NoteNamespace>> QueryAsync(
        NoteNamespaceQuery query,
        CancellationToken cancellationToken = default);
}
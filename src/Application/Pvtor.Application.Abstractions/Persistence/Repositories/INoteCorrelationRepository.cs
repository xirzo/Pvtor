using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteCorrelationRepository
{
    Task<NoteCorrelation> AddAsync(NoteCorrelation noteCorrelation, CancellationToken cancellationToken = default);

    Task<IEnumerable<NoteCorrelation>> Query(NoteCorrelationQuery query, CancellationToken cancellationToken = default);
}
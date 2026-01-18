using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes.Correlations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteCorrelationRepository
{
    Task AddAsync(NoteCorrelation noteCorrelation, CancellationToken cancellationToken = default);

    Task<IEnumerable<NoteCorrelation>> QueryAsync(
        NoteCorrelationQuery query,
        CancellationToken cancellationToken = default);
}
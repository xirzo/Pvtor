using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Sqlite.Repositories;

public class SqliteNoteCorrelationRepository : INoteCorrelationRepository
{
    private readonly string _connectionString;

    public SqliteNoteCorrelationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<NoteCorrelation> AddAsync(
        NoteCorrelation noteCorrelation,
        CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<NoteCorrelation>> Query(
        NoteCorrelationQuery query,
        CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
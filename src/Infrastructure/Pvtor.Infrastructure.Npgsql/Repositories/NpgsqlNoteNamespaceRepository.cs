using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes.Namespaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pvtor.Infrastructure.Npgsql.Repositories;

public class NpgsqlNoteNamespaceRepository : INoteNamespaceRepository
{
    private readonly string _connectionString;

    public NpgsqlNoteNamespaceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task<NoteNamespace> AddAsync(NoteNamespace noteNamespace)
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<NoteNamespace>> QueryAsync(NoteNamespaceQuery query)
    {
        throw new System.NotImplementedException();
    }
}
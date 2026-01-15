using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;

namespace Pvtor.Infrastructure.Sqlite;

internal sealed class SqlitePersistenceContext : IPersistanceContext
{
    public SqlitePersistenceContext(INoteRepository noteRepository)
    {
        NoteRepository = noteRepository;
    }

    public INoteRepository NoteRepository { get; }
}
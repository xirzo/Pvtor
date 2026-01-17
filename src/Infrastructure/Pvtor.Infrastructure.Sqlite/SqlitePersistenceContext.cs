using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;

namespace Pvtor.Infrastructure.Sqlite;

internal sealed class SqlitePersistenceContext : IPersistanceContext
{
    public SqlitePersistenceContext(
        INoteCorrelationRepository noteCorrelationRepository,
        INoteRepository noteRepository)
    {
        NoteCorrelationRepository = noteCorrelationRepository;
        NoteRepository = noteRepository;
    }

    public INoteCorrelationRepository NoteCorrelationRepository { get; }

    public INoteRepository NoteRepository { get; }
}
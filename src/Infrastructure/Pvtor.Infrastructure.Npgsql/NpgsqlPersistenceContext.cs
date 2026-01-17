using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;

namespace Pvtor.Infrastructure.Npgsql;

public class NpgsqlPersistenceContext : IPersistanceContext
{
    public NpgsqlPersistenceContext(INoteCorrelationRepository noteCorrelationRepository, INoteRepository noteRepository)
    {
        NoteCorrelationRepository = noteCorrelationRepository;
        NoteRepository = noteRepository;
    }

    public INoteCorrelationRepository NoteCorrelationRepository { get; }

    public INoteRepository NoteRepository { get; }
}
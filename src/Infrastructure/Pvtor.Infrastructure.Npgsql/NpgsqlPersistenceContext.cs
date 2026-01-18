using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;

namespace Pvtor.Infrastructure.Npgsql;

public class NpgsqlPersistenceContext : IPersistanceContext
{
    public NpgsqlPersistenceContext(
        INoteCorrelationRepository noteCorrelationRepository,
        INoteRepository noteRepository,
        INoteChannelRepository noteChannelRepository)
    {
        NoteCorrelationRepository = noteCorrelationRepository;
        NoteRepository = noteRepository;
        NoteChannelRepository = noteChannelRepository;
    }

    public INoteCorrelationRepository NoteCorrelationRepository { get; }

    public INoteRepository NoteRepository { get; }

    public INoteChannelRepository NoteChannelRepository { get; }
}
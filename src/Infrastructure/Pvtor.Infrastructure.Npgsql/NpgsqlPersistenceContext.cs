using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;

namespace Pvtor.Infrastructure.Npgsql;

public class NpgsqlPersistenceContext : IPersistanceContext
{
    public NpgsqlPersistenceContext(
        INoteCorrelationRepository noteCorrelationRepository,
        INoteRepository noteRepository,
        INoteChannelRepository noteChannelRepository,
        INoteNamespaceRepository noteNamespaceRepository)
    {
        NoteCorrelationRepository = noteCorrelationRepository;
        NoteRepository = noteRepository;
        NoteChannelRepository = noteChannelRepository;
        NoteNamespaceRepository = noteNamespaceRepository;
    }

    public INoteCorrelationRepository NoteCorrelationRepository { get; }

    public INoteRepository NoteRepository { get; }

    public INoteChannelRepository NoteChannelRepository { get; }

    public INoteNamespaceRepository NoteNamespaceRepository { get; }
}
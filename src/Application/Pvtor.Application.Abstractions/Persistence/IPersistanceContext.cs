using Pvtor.Application.Abstractions.Persistence.Repositories;

namespace Pvtor.Application.Abstractions.Persistence;

public interface IPersistanceContext
{
    INoteRepository NoteRepository { get;  }
}
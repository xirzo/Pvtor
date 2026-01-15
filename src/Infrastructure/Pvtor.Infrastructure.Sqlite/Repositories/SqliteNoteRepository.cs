using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Domain.Notes;
using System.Collections.Generic;

namespace Pvtor.Infrastructure.Sqlite.Repositories;

internal sealed class SqliteNoteRepository : INoteRepository
{
    public Note Add(Note note)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerable<Note> Query(NoteQuery query)
    {
        throw new System.NotImplementedException();
    }
}
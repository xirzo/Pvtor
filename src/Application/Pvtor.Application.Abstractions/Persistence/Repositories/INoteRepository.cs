using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes;
using System.Collections.Generic;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteRepository
{
    Note Add(Note note);

    IEnumerable<Note> Query(NoteQuery query);
}
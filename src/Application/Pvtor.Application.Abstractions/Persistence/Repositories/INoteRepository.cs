using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteRepository
{
    Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default);

    IEnumerable<Note> Query(NoteQuery query);
}
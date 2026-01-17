using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteRepository
{
    Task<Note> AddAsync(Note note, CancellationToken cancellationToken = default);

    Task<IEnumerable<Note>> QueryAsync(NoteQuery query, CancellationToken cancellationToken = default);

    Task<Note> UpdateAsync(Note note, CancellationToken cancellationToken = default);
}
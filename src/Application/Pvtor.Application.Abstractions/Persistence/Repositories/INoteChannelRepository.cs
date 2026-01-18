using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Domain.Notes.Channels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Abstractions.Persistence.Repositories;

public interface INoteChannelRepository
{
    Task<NoteChannel> AddAsync(
        NoteChannel channel,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NoteChannel>> QueryAsync(
        NoteChannelQuery query,
        CancellationToken cancellationToken = default);

    Task RemoveBySourceChannelIdAsync(string noteSourceChannelId, CancellationToken cancellationToken);
}
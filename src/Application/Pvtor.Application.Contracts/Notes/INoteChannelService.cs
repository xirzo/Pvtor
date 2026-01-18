using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteChannelService
{
    Task<RegisterChannel.Response> RegisterChannelAsync(
        RegisterChannel.Request request,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NoteChannelDto>> GetAll();

    Task<NoteChannelDto?> FindBySourceChannelIdAsync(string sourceChatId);
}
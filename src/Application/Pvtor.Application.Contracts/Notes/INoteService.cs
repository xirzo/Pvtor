using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteService
{
    Task<CreateNote.Response> CreateNoteAsync(
        CreateNote.Request request,
        CancellationToken cancellationToken = default);

    Task<UpdateNote.Response> UpdateNoteAsync(
        UpdateNote.Request request,
        CancellationToken cancellationToken = default);

    INoteChangeSubscription AddSubscriber(INoteChangedSubscriber subscriber);

    Task<MarkNoteAsHidden.Response> MarkNoteAsHidden(
        MarkNoteAsHidden.Request request,
        CancellationToken cancellationToken = default);

    // TODO: replace with query
    Task<IEnumerable<NoteDto>> GetAllAsync();

    Task<IEnumerable<NoteDto>> GetAllByChannelId(long channelNoteChannelId);

    Task<IEnumerable<NoteDto>> GetAllByNamespaceId(long? channelNoteNamespaceId);
}
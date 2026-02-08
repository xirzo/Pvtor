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

    Task<IEnumerable<NoteDto>> QueryAsync(NoteDtoQuery query, CancellationToken cancellationToken);

    // TODO: replace with query
    Task<IEnumerable<NoteDto>> GetNonHiddenByNamespaceId(long? channelNoteNamespaceId);

    Task<DeleteNote.Response> DeleteNoteAsync(
        DeleteNote.Request request,
        CancellationToken cancellationToken = default);
}
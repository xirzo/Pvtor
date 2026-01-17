using Pvtor.Application.Contracts.Notes.Operations;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteService
{
    Task<CreateNote.Response> CreateNoteAsync(
        CreateNote.Request request,
        CancellationToken cancellationToken = default);

    Task<UpdateNote.Response> UpdateNodeAsync(
        UpdateNote.Request request,
        CancellationToken cancellationToken = default);
}
using Pvtor.Application.Contracts.Notes.Operations;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteService
{
    CreateNote.Response CreateNote(CreateNote.Request request);
}
using Pvtor.Application.Contracts.Notes.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteChangedSubscriber
{
    Task OnNoteChanged(NoteDto note, CancellationToken cancellationToken);
}
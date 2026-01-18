using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Threading.Tasks;

namespace Pvtor.Application.Contracts.Notes;

public interface INoteNamespaceService
{
    Task<CreateNamespace.Response> CreateAsync(CreateNamespace.Request request);

    Task<NoteNamespaceDto> FindByNameAsync(string name);
}
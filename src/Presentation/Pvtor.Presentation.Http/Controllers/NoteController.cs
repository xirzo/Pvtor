using Microsoft.AspNetCore.Mvc;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Presentation.Http.Models;
using Pvtor.Presentation.Http.Parameters;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Presentation.Http.Controllers;

[ApiController]
[Route("api/notes")]
public class NoteController : ControllerBase
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> CreateNoteAsync(
        [FromBody] CreateNoteRequest httpRequest,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateNote.Request(httpRequest.Content, httpRequest.Name, httpRequest.NamespaceId);
        CreateNote.Response response = await _noteService.CreateNoteAsync(request, cancellationToken);

        return response switch
        {
            CreateNote.Response.PersistenceFailure persistenceFailure => BadRequest(persistenceFailure.Message),
            CreateNote.Response.Success success => Ok(success.Note),
            _ => throw new UnreachableException(),
        };
    }

    [HttpGet]
    public async Task<ActionResult<NoteDto>> QueryNotesAsync(
        [FromQuery] QueryNotesParameters? parameters,
        CancellationToken cancellationToken = default)
    {
        QueryNotesParameters safeParameters = parameters ?? new QueryNotesParameters();

        IEnumerable<NoteDto> response = await _noteService.QueryAsync(
            NoteDtoQuery.Build(builder =>
                builder
                    .WithIds(safeParameters.NoteIds)
                    .WithNamespaceIds(safeParameters.NamespaceIds)
                    .OnlyNonHidden(safeParameters.OnlyNonHidden)
                    .WithSortOrder(safeParameters.SortOrder)
                    .WithContent(safeParameters.Content)),
            cancellationToken);

        return Ok(response);
    }

    [HttpPatch("{noteId}")]
    public async Task<ActionResult<NoteDto>> UpdateNoteAsync(
        long noteId,
        [FromBody] UpdateNoteRequest httpRequest,
        CancellationToken cancellationToken = default)
    {
        var request = new UpdateNote.Request(noteId, httpRequest.Content, httpRequest.Name);
        UpdateNote.Response response = await _noteService.UpdateNoteAsync(request, cancellationToken);

        return response switch
        {
            UpdateNote.Response.NotFound notFound => NotFound(notFound.Message),
            UpdateNote.Response.PersistenceFailure persistenceFailure => BadRequest(persistenceFailure.Message),
            UpdateNote.Response.Success success => Ok(success.Note),
            _ => throw new UnreachableException(),
        };
    }

    [HttpDelete("{noteId}")]
    public async Task<ActionResult<NoteDto>> DeleteNoteAsync(long noteId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteNote.Request(noteId);
        DeleteNote.Response response = await _noteService.DeleteNoteAsync(request, cancellationToken);

        return response switch
        {
            DeleteNote.Response.PersistenceFailure persistenceFailure => BadRequest(persistenceFailure.Message),
            DeleteNote.Response.Success => Ok(),
            _ => throw new UnreachableException(),
        };
    }
}
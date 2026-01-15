using Microsoft.AspNetCore.Mvc;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Presentation.Http.Models;
using System.Diagnostics;

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
    public ActionResult<NoteDto> CreateNote([FromBody] CreateNoteRequest httpRequest)
    {
        var request = new CreateNote.Request(httpRequest.Content);
        CreateNote.Response response = _noteService.CreateNote(request);

        return response switch {
            CreateNote.Response.PersistenceFailure persistenceFailure => BadRequest(persistenceFailure.Message),
            CreateNote.Response.Success success => Ok(success.Note),
            _ => throw new UnreachableException(),
        };
    }
}

using Microsoft.AspNetCore.Mvc;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Presentation.Http.Parameters;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Presentation.Http.Controllers;

[ApiController]
[Route("api/namespaces")]
public class NamespaceController : ControllerBase
{
    private readonly INoteNamespaceService _namespaceService;

    public NamespaceController(INoteNamespaceService namespaceService)
    {
        _namespaceService = namespaceService;
    }

    [HttpGet]
    public async Task<ActionResult<NoteNamespaceDto>> QueryNamespacesAsync(
        [FromQuery] QueryNamespacesParameters? parameters,
        CancellationToken cancellationToken = default)
    {
        QueryNamespacesParameters safeParameters = parameters ?? new();

        IEnumerable<NoteNamespaceDto> response = await _namespaceService.QueryAsync(
            NamespaceDtoQuery.Build(builder =>
                builder
                    .WithNamespaceIds(safeParameters.NamespaceIds)),
            cancellationToken);

        return Ok(response);
    }
}
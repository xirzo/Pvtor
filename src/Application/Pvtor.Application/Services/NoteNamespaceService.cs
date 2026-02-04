using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes.Namespaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Services;

public class NoteNamespaceService : INoteNamespaceService
{
    private readonly IPersistanceContext _context;

    public NoteNamespaceService(IPersistanceContext context)
    {
        _context = context;
    }

    public async Task<CreateNamespace.Response> CreateAsync(CreateNamespace.Request request)
    {
        try
        {
            NoteNamespace noteNamespace = await _context.NoteNamespaceRepository.AddAsync(new NoteNamespace(
                NoteNamespaceId.Default,
                request.Name,
                DateTime.UtcNow));

            return new CreateNamespace.Response.Success(noteNamespace.MapToDto());
        }
        catch (Exception ex)
        {
            return new CreateNamespace.Response.PersistenceFailure(ex.Message);
        }
    }

    // TODO: replace by query
    public async Task<NoteNamespaceDto?> FindByNameAsync(string name)
    {
        return (await _context.NoteNamespaceRepository.QueryAsync(NoteNamespaceQuery.Build(builder =>
                builder.WithName(name)))).SingleOrDefault()
            ?.MapToDto();
    }

    public async Task<IEnumerable<NoteNamespaceDto>> QueryAsync(NamespaceDtoQuery query, CancellationToken cancellationToken)
    {
        NoteNamespaceId[] namespaceIds = query.NamespaceIds
            .Select(id => new NoteNamespaceId(id))
            .ToArray();

        var namespaceQuery = NoteNamespaceQuery.Build(builder => builder
                .WithNoteNamespaceIds(namespaceIds));

        return (await _context.NoteNamespaceRepository.QueryAsync(
                namespaceQuery,
                cancellationToken))
            .Select(x => x.MapToDto());
    }
}
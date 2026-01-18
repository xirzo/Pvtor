using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes.Channels;
using Pvtor.Domain.Notes.Namespaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Services;

public class NoteChannelService : INoteChannelService
{
    private readonly IPersistanceContext _context;

    public NoteChannelService(IPersistanceContext context)
    {
        _context = context;
    }

    public async Task<RegisterChannel.Response> RegisterChannelAsync(
        RegisterChannel.Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: May cause a problem? Need to check if namespace exists and return an error
            NoteNamespaceId? noteNamespaceId = request.NoteNamespaceId is null
                ? null
                : new NoteNamespaceId(request.NoteNamespaceId.Value);

            NoteChannel channel = await _context.NoteChannelRepository.AddAsync(
                new NoteChannel(NoteChannelId.Default, request.SourceChannelId, DateTime.UtcNow, noteNamespaceId),
                cancellationToken);

            return new RegisterChannel.Response.Success(channel.MapToDto());
        }
        catch (Exception ex)
        {
            return new RegisterChannel.Response.PersistenceFailure(ex.Message);
        }
    }

    public async Task<UnregisterChannel.Response> UnregisterChannelAsync(
        UnregisterChannel.Request request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _context.NoteChannelRepository.RemoveBySourceChannelIdAsync(
                request.SourceChannelId,
                cancellationToken);

            return new UnregisterChannel.Response.Success();
        }
        catch (Exception ex)
        {
            return new UnregisterChannel.Response.PersistenceFailure(ex.Message);
        }
    }

    public async Task<IEnumerable<NoteChannelDto>> GetAllAsync()
    {
        var query = NoteChannelQuery.Build(builder => builder
            .WithIds([])
            .WithNoteSourceChannelIds([]));

        IEnumerable<NoteChannel> channels = await _context.NoteChannelRepository.QueryAsync(query);

        return channels.Select(x => x.MapToDto());
    }

    public async Task<NoteChannelDto?> FindBySourceChannelIdAsync(string sourceChatId)
    {
        return (await _context.NoteChannelRepository.QueryAsync(NoteChannelQuery.Build(builder =>
                builder.WithNoteSourceChannelId(sourceChatId))))
            .SingleOrDefault()
            ?.MapToDto();
    }
}
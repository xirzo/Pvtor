using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes.Channels;
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
            NoteChannel channel = await _context.NoteChannelRepository.AddAsync(
                new NoteChannel(NoteChannelId.Default, request.SourceChannelId, DateTime.UtcNow),
                cancellationToken);

            return new RegisterChannel.Response.Success(channel.MapToDto());
        }
        catch (Exception ex)
        {
            return new RegisterChannel.Response.PersistenceFailure(ex.Message);
        }
    }

    public async Task<IEnumerable<NoteChannelDto>> GetAll()
    {
        var query = NoteChannelQuery.Build(builder => builder
            .WithIds([])
            .WithNoteSourceChannelIds([]));

        IEnumerable<NoteChannel> channels = await _context.NoteChannelRepository.QueryAsync(query);

        return channels.Select(x => x.MapToDto());
    }
}
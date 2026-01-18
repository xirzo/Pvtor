using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes;
using Pvtor.Domain.Notes.Channels;
using Pvtor.Domain.Notes.Namespaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Application.Services;

internal sealed class NoteService : INoteService
{
    private readonly IPersistanceContext _context;
    private readonly List<INoteChangedSubscriber> _subscribers = [];

    public NoteService(IPersistanceContext context)
    {
        _context = context;
    }

    public async Task<CreateNote.Response> CreateNoteAsync(
        CreateNote.Request request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            NoteNamespaceId noteNamespace = request.NamespaceId is null
                ? NoteNamespaceId.Default
                : new NoteNamespaceId(request.NamespaceId.Value);

            Note note = await _context.NoteRepository.AddAsync(
                new Note(NoteId.Default, request.Content, DateTime.UtcNow, DateTime.UtcNow, noteNamespace),
                cancellationToken);

            NoteDto noteDto = note.MapToDto();

            foreach (INoteChangedSubscriber subscriber in _subscribers)
            {
                await subscriber.OnNoteChanged(noteDto);
            }

            return new CreateNote.Response.Success(noteDto);
        }
        catch (Exception ex)
        {
            return new CreateNote.Response.PersistenceFailure(ex.Message);
        }
    }

    public async Task<UpdateNote.Response> UpdateNodeAsync(
        UpdateNote.Request request,
        CancellationToken cancellationToken = default)
    {
        Note? note = (await _context.NoteRepository.QueryAsync(
            NoteQuery.Build(builder =>
                builder.WithId(new NoteId(request.NoteId))),
            cancellationToken)).SingleOrDefault();

        if (note is null)
        {
            return new UpdateNote.Response.NotFound($"Note with id: {request.NoteId} is not found");
        }

        Note updatedNote =
            await _context.NoteRepository.UpdateAsync(note with { Content = request.Content }, cancellationToken);

        NoteDto noteDto = updatedNote.MapToDto();

        foreach (INoteChangedSubscriber subscriber in _subscribers)
        {
            await subscriber.OnNoteChanged(noteDto);
        }

        return new UpdateNote.Response.Success(noteDto);
    }

    public INoteChangeSubscription AddSubscriber(INoteChangedSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        return new Subscription(this, subscriber);
    }

    public async Task<IEnumerable<NoteDto>> GetAllAsync()
    {
        return (await _context.NoteRepository.QueryAsync(NoteQuery.Build(builder => builder.WithIds([]))))
            .Select(x => x.MapToDto());
    }

    public async Task<IEnumerable<NoteDto>> GetAllByChannelId(long channelNoteChannelId)
    {
        return (await _context.NoteRepository.QueryAsync(NoteQuery.Build(builder =>
                builder.WithNoteChannelId(new NoteChannelId(channelNoteChannelId)))))
            .Select(x => x.MapToDto());
    }

    public async Task<IEnumerable<NoteDto>> GetAllByNamespaceId(long? channelNoteNamespaceId)
    {
        if (channelNoteNamespaceId is null)
        {
            // TODO: this returns all of the namespaces?
            return (await _context.NoteRepository.QueryAsync(
                    NoteQuery.Build(builder => builder.WithNoteNamespaceIds([]))))
                .Select(x => x.MapToDto());
        }

        return (await _context.NoteRepository.QueryAsync(NoteQuery.Build(builder =>
                builder.WithNoteNamespaceId(new NoteNamespaceId(channelNoteNamespaceId.Value)))))
            .Select(x => x.MapToDto());
    }

    private class Subscription(NoteService service, INoteChangedSubscriber subscriber) : INoteChangeSubscription
    {
        public void Unsubscribe()
        {
            service._subscribers.Remove(subscriber);
        }
    }
}
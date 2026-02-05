using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Queries;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using Pvtor.Application.Mapping;
using Pvtor.Domain.Notes;
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
                new Note(
                    NoteId.Default,
                    request.Name,
                    request.Content,
                    DateTime.UtcNow,
                    DateTime.UtcNow,
                    noteNamespace,
                    false),
                cancellationToken);

            NoteDto noteDto = note.MapToDto();

            foreach (INoteChangedSubscriber subscriber in _subscribers)
            {
                await subscriber.OnNoteChanged(noteDto, cancellationToken);
            }

            return new CreateNote.Response.Success(noteDto);
        }
        catch (Exception ex)
        {
            return new CreateNote.Response.PersistenceFailure(ex.Message);
        }
    }

    public async Task<UpdateNote.Response> UpdateNoteAsync(
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
            await subscriber.OnNoteChanged(noteDto, cancellationToken);
        }

        return new UpdateNote.Response.Success(noteDto);
    }

    public INoteChangeSubscription AddSubscriber(INoteChangedSubscriber subscriber)
    {
        _subscribers.Add(subscriber);
        return new Subscription(this, subscriber);
    }

    public async Task<MarkNoteAsHidden.Response> MarkNoteAsHidden(
        MarkNoteAsHidden.Request request,
        CancellationToken cancellationToken = default)
    {
        Note? note = (await _context.NoteRepository.QueryAsync(
            NoteQuery.Build(builder =>
                builder.WithId(new NoteId(request.NoteId))),
            cancellationToken)).SingleOrDefault();

        if (note is null)
        {
            return new MarkNoteAsHidden.Response.NotFound($"Note with id: {request.NoteId} is not found");
        }

        Note updatedNote =
            await _context.NoteRepository.UpdateAsync(note with { IsHidden = true }, cancellationToken);

        NoteDto noteDto = updatedNote.MapToDto();

        foreach (INoteChangedSubscriber subscriber in _subscribers)
        {
            await subscriber.OnNoteChanged(noteDto, cancellationToken);
        }

        return new MarkNoteAsHidden.Response.Success();
    }

    public async Task<IEnumerable<NoteDto>> QueryAsync(NoteDtoQuery query, CancellationToken cancellationToken)
    {
        NoteId[] noteIds = query.NoteIds
            .Select(id => new NoteId(id))
            .ToArray();

        bool useNullNamespace = query.NamespaceIds
            .Any(id => id == 0);

        NoteNamespaceId[] namespaceIds = query.NamespaceIds
            .Where(id => id != 0)
            .Select(id => new NoteNamespaceId(id))
            .ToArray();

        var noteQuery = NoteQuery.Build(builder =>
            builder
                .WithIds(noteIds)
                .WithNoteNamespaceIds(namespaceIds)
                .WithUseNullNamespace(useNullNamespace)
                .WithContent(query.Content)
                .WithOnlyNonHidden(query.OnlyNonHidden));

        return (await _context.NoteRepository.QueryAsync(
                noteQuery,
                cancellationToken))
            .Select(x => x.MapToDto());
    }

    public async Task<IEnumerable<NoteDto>> GetNonHiddenByNamespaceId(long? channelNoteNamespaceId)
    {
        if (channelNoteNamespaceId is null)
        {
            return (await _context.NoteRepository.QueryAsync(
                    NoteQuery.Build(builder => builder.WithUseNullNamespace(true).WithOnlyNonHidden(true))))
                .Select(x => x.MapToDto());
        }

        return (await _context.NoteRepository.QueryAsync(NoteQuery.Build(builder =>
                builder.WithNoteNamespaceId(new NoteNamespaceId(channelNoteNamespaceId.Value))
                    .WithOnlyNonHidden(true))))
            .Select(x => x.MapToDto());
    }

    public async Task<DeleteNote.Response> DeleteAsync(long noteId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.NoteRepository.DeleteAsync(new NoteId(noteId), cancellationToken);
            return new DeleteNote.Response.Success();
        }
        catch (Exception ex)
        {
            return new DeleteNote.Response.PersistenceFailure(ex.Message);
        }
    }

    private class Subscription(NoteService service, INoteChangedSubscriber subscriber) : INoteChangeSubscription
    {
        public void Unsubscribe()
        {
            service._subscribers.Remove(subscriber);
        }
    }
}
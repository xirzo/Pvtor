using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot;

public class BotUpdateHandler : IUpdateHandler, INoteChangedSubscriber
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<BotUpdateHandler> _logger;
    private readonly INoteService _noteService;
    private readonly INoteCorrelationService _correlationService;
    private readonly INoteChannelService _channelService;

    public BotUpdateHandler(
        ITelegramBotClient bot,
        ILogger<BotUpdateHandler> logger,
        INoteService noteService,
        INoteCorrelationService correlationService,
        INoteChannelService channelService)
    {
        _bot = bot;
        _logger = logger;
        _noteService = noteService;
        _correlationService = correlationService;
        _channelService = channelService;
        _noteService.AddSubscriber(this);
    }

    public async Task OnNoteChanged(NoteDto note)
    {
        _logger.LogInformation(
            $"Note with id {note.NoteId} was changed, syncing telegram...");

        var correlations =
            (await _correlationService.FindByNoteIdAsync(note.NoteId))
            .ToList();

        var allChats = (await _channelService.GetAll())
            .ToList();

        var chatsWithExistingCorrelation = new List<(NoteChannelDto, NoteCorrelationDto)>();
        var noCorrelationChats = new List<NoteChannelDto>();

        foreach (NoteChannelDto chat in allChats)
        {
            NoteCorrelationDto? correlation = correlations.FirstOrDefault(x => x.NoteChannelId == chat.NoteChannelId);
            if (correlation != null)
            {
                chatsWithExistingCorrelation.Add((chat, correlation));
            }
            else
            {
                noCorrelationChats.Add(chat);
            }
        }

        foreach ((NoteChannelDto chat, NoteCorrelationDto correlation) in chatsWithExistingCorrelation)
        {
            // FIX: unsafe
            int messageId = Convert.ToInt32(correlation.NoteSourceId);

            await _bot.EditMessageText(
                new ChatId(chat.NoteSourceChannelId),
                messageId,
                note.Content);
        }

        foreach (NoteChannelDto chat in noCorrelationChats)
        {
            Message message =
                await _bot.SendMessage(new ChatId(chat.NoteSourceChannelId), note.Content);

            await RecordCorrelation(message, note.NoteId);
        }
    }

    public async Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);

        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await (update switch
        {
            { ChannelPost: { } message } => OnMessage(message, cancellationToken),
            { EditedChannelPost: { } message } => OnMessageEdited(message, cancellationToken),
            { Message: { } message } => OnMessage(message, cancellationToken),
            { EditedMessage: { } message } => OnMessageEdited(message, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update),
        });
    }

    private async Task OnMessage(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        string[] words = message.Text.Split(' ');

        // TODO: if some tryhard parsing is required use CoR from OOP labwork4
        if (words[0] is "/register")
        {
            await _channelService.RegisterChannelAsync(
                new RegisterChannel.Request(message.Chat.Id.ToString()),
                cancellationToken);
        }

        CreateNote.Response createNoteResponse =
            await _noteService.CreateNoteAsync(
                new CreateNote.Request(messageText),
                cancellationToken);

        switch (createNoteResponse)
        {
            case CreateNote.Response.PersistenceFailure persistenceFailure:
                _logger.LogError(
                    $"Failed to save message with id: {message.Id}, persistence failure: {persistenceFailure.Message}");
                break;
            case CreateNote.Response.Success createSuccess:
                await RecordCorrelation(message, createSuccess.Note.NoteId, cancellationToken);
                break;
        }

        await _bot.SendMessage(message.Chat, "DEBUG: Saved message", cancellationToken: cancellationToken);
    }

    private async Task RecordCorrelation(Message message, long noteId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Saved message with id: {message.Id} as note with id: {noteId}");

        RecordCorrelation.Response recordResponse =
            await _correlationService.RecordCorrelationAsync(
                new RecordCorrelation.Request(noteId, message.Id.ToString(), message.Chat.Id),
                cancellationToken);

        switch (recordResponse)
        {
            case RecordCorrelation.Response.PersistenceFailure recordPersistenceFailure:
                _logger.LogError(
                    $"Failed to record correlation for message with id: {message.Id}, persistence failure: {recordPersistenceFailure.Message}");
                break;
            case Application.Contracts.Notes.Operations.RecordCorrelation.Response.Success:
                _logger.LogInformation(
                    $"Saved correlation for message with id: {message.Id} (note with id: {noteId})");
                break;
        }
    }

    private async Task OnMessageEdited(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive edited message, type: {MessageType}", message.Type);
        if (message.Text is not { } newMessageText)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        NoteCorrelationDto? correlations =
            (await _correlationService.FindBySourceIdAsync(message.Id.ToString()))
            .SingleOrDefault();

        if (correlations is null)
        {
            _logger.LogInformation("Message with id: {MessageId} doesn't exist", message.Id);
            await OnMessage(message, cancellationToken);
            return;
        }

        UpdateNote.Response updateNoteResponse =
            await _noteService.UpdateNodeAsync(
                new UpdateNote.Request(correlations.NoteId, newMessageText),
                cancellationToken);

        switch (updateNoteResponse)
        {
            case UpdateNote.Response.PersistenceFailure persistenceFailure:
                _logger.LogInformation(
                    $"Failed to update message with id: {message.Id}, persistence failure: {persistenceFailure.Message}");
                break;
            case UpdateNote.Response.Success:
                _logger.LogInformation($"Updated message with id: {message.Id}");
                break;
        }

        await _bot.SendMessage(message.Chat, "DEBUG: Edited message", cancellationToken: cancellationToken);
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
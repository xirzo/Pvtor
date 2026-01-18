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
        _logger.LogInformation($"Note with id {note.NoteId} was changed, syncing telegram...");

        IEnumerable<NoteCorrelationDto> correlations = await _correlationService.FindByNoteIdAsync(note.NoteId);
        var correlationMap = correlations.ToDictionary(x => x.NoteChannelId);

        IEnumerable<NoteChannelDto> allChats = await _channelService.GetAll();

        foreach (NoteChannelDto chat in allChats)
        {
            if (correlationMap.TryGetValue(chat.NoteChannelId, out NoteCorrelationDto? correlation))
            {
                await UpdateMessageAsync(chat, correlation, note);
                continue;
            }

            await SendMessageAsync(chat, note);
        }
    }

    public async Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"HandleError: {exception}");

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

    private async Task OnMessageEdited(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Receive edited message, type: {message.Type}");
        if (message.Text is not { } newMessageText)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        NoteCorrelationDto? correlation = (await _correlationService.FindBySourceIdAsync(message.Id.ToString()))
            .SingleOrDefault();

        if (correlation is null)
        {
            _logger.LogInformation($"Message with id: {message.Id} doesn't exist");
            await OnMessage(message, cancellationToken);
            return;
        }

        UpdateNote.Response updateResponse = await _noteService.UpdateNodeAsync(
            new UpdateNote.Request(correlation.NoteId, newMessageText),
            cancellationToken);

        if (updateResponse is UpdateNote.Response.PersistenceFailure failure)
        {
            _logger.LogInformation(
                $"Failed to update message with id: {message.Id}, persistence failure: {failure.Message}");
            return;
        }

        _logger.LogInformation($"Updated message with id: {message.Id}");
    }

    private async Task OnMessage(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Receive message type: {message.Type}");
        if (message.Text is not { } messageText)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        string[] words = messageText.Split(' ');

        if (words[0] is "/register")
        {
            await RegisterChannelAsync(message, cancellationToken);
            return;
        }

        if (words[0] is "/unregister")
        {
            await UnregisterChannelAsync(message, cancellationToken);
            return;
        }

        NoteChannelDto? currentChannel = await _channelService.FindBySourceChannelIdAsync(message.Chat.Id.ToString());

        if (currentChannel is null)
        {
            _logger.LogInformation(
                $"Message was skipped, as a telegram chat with id {message.Chat.Id} is not registered...");
            return;
        }

        CreateNote.Response createResponse = await _noteService.CreateNoteAsync(
            new CreateNote.Request(messageText),
            cancellationToken);

        switch (createResponse)
        {
            case CreateNote.Response.PersistenceFailure failure:
                _logger.LogError(
                    $"Failed to save message with id: {message.Id}, persistence failure: {failure.Message}");
                break;
            case CreateNote.Response.Success:
                try
                {
                    await _bot.DeleteMessage(message.Chat.Id, message.Id, cancellationToken);
                    _logger.LogInformation(
                        $"Deleted user message with id: {message.Id} in chat with id: {message.Chat.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete original message (might lack permissions)");
                }

                break;
        }
    }

    private async Task SendMessageAsync(NoteChannelDto chat, NoteDto note)
    {
        try
        {
            Message message = await _bot.SendMessage(new ChatId(chat.NoteSourceChannelId), note.Content);
            _logger.LogInformation($"Sent message to chat with id: {chat.NoteSourceChannelId}");

            await RecordCorrelation(message, note.NoteId, chat.NoteChannelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send/record message in chat {chat.NoteSourceChannelId}");
        }
    }

    private async Task RegisterChannelAsync(Message message, CancellationToken cancellationToken)
    {
        await _channelService.RegisterChannelAsync(
            new RegisterChannel.Request(message.Chat.Id.ToString()),
            cancellationToken);

        await _bot.SendMessage(
            message.Chat,
            "Successfully registered the chat",
            cancellationToken: cancellationToken);
    }

    private async Task UnregisterChannelAsync(Message message, CancellationToken cancellationToken)
    {
        await _channelService.UnregisterChannelAsync(
            new UnregisterChannel.Request(message.Chat.Id.ToString()),
            cancellationToken);

        await _bot.SendMessage(
            message.Chat,
            "Successfully unregistered the chat",
            cancellationToken: cancellationToken);
    }

    private async Task UpdateMessageAsync(NoteChannelDto chat, NoteCorrelationDto correlation, NoteDto note)
    {
        if (!int.TryParse(correlation.NoteSourceId, out int messageId))
        {
            _logger.LogError(
                $"Correlation found for Channel {chat.NoteChannelId}, but MessageId '{correlation.NoteSourceId}' is not a valid integer.");
            return;
        }

        try
        {
            await _bot.EditMessageText(
                new ChatId(chat.NoteSourceChannelId),
                messageId,
                note.Content);
            _logger.LogInformation($"Edited message in the chat with id: {chat.NoteSourceChannelId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to edit message in chat {chat.NoteSourceChannelId}");
        }
    }

    private async Task RecordCorrelation(
        Message message,
        long noteId,
        long channelId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Saved message with id: {message.Id} as note with id: {noteId}");

        RecordCorrelation.Response response = await _correlationService.RecordCorrelationAsync(
            new RecordCorrelation.Request(noteId, message.Id.ToString(), channelId),
            cancellationToken);

        if (response is RecordCorrelation.Response.PersistenceFailure failure)
        {
            _logger.LogError(
                $"Failed to record correlation for message with id: {message.Id}, persistence failure: {failure.Message}");
        }
        else
        {
            _logger.LogInformation($"Saved correlation for message with id: {message.Id} (note with id: {noteId})");
        }
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}
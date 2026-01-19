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
    private readonly INoteNamespaceService _namespaceService;

    public BotUpdateHandler(
        ITelegramBotClient bot,
        ILogger<BotUpdateHandler> logger,
        INoteService noteService,
        INoteCorrelationService correlationService,
        INoteChannelService channelService,
        INoteNamespaceService namespaceService)
    {
        _bot = bot;
        _logger = logger;
        _noteService = noteService;
        _correlationService = correlationService;
        _channelService = channelService;
        _namespaceService = namespaceService;
        _noteService.AddSubscriber(this);
    }

    public async Task OnNoteChanged(NoteDto note)
    {
        _logger.LogInformation($"Note with id {note.NoteId} was changed, syncing telegram...");

        IEnumerable<NoteCorrelationDto> correlations = await _correlationService.FindByNoteIdAsync(note.NoteId);
        var correlationMap = correlations.ToDictionary(x => x.NoteChannelId);

        IEnumerable<NoteChannelDto> allChats = await _channelService.GetAllAsync();

        foreach (NoteChannelDto chat in allChats)
        {
            // FIX: Ensure we only broadcast to channels within the same namespace
            if (chat.NoteNamespaceId != note.NoteNamespaceId)
            {
                continue;
            }

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
            { Message: { } message } => OnMessage(message, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update),
        });
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

        if (words[0] == "/register")
        {
            await _bot.DeleteMessage(message.Chat.Id, message.Id, cancellationToken);
            _logger.LogInformation(
                $"Deleted user message with id: {message.Id} in chat with id: {message.Chat.Id}");

            if (words.Length == 1)
            {
                _logger.LogInformation("Register command did not provide a namespace, using default...");
                await RegisterChannelAsync(message, null, cancellationToken);
                return;
            }

            if (words.Length == 2)
            {
                string namespaceName = words[1];
                NoteNamespaceDto? noteNamespace = await _namespaceService.FindByNameAsync(namespaceName);

                if (noteNamespace is null)
                {
                    _logger.LogInformation($"Namespace with name: {namespaceName} does not exist, creating a new...");
                    CreateNamespace.Response response =
                        await _namespaceService.CreateAsync(new CreateNamespace.Request(namespaceName));

                    switch (response)
                    {
                        case CreateNamespace.Response.PersistenceFailure persistenceFailure:
                            _logger.LogError(
                                $"Failed to create a namespace with name: {namespaceName}, error: {persistenceFailure.Message}");
                            return;
                        case CreateNamespace.Response.Success success:
                            noteNamespace = success.Namespace;
                            _logger.LogInformation($"Successfully created a new namespace with name: {namespaceName}");
                            break;
                    }
                }

                await RegisterChannelAsync(message, noteNamespace?.NoteNamespaceId, cancellationToken);
                return;
            }
        }
        else if (words[0] == "/unregister")
        {
            await _bot.DeleteMessage(message.Chat.Id, message.Id, cancellationToken);
            _logger.LogInformation(
                $"Deleted user message with id: {message.Id} in chat with id: {message.Chat.Id}");
            await UnregisterChannelAsync(message, cancellationToken);
            return;
        }
        else if (words[0] == "/edit" && message.ReplyToMessage is { } replyToMessage)
        {
            _logger.LogInformation($"Receive edit command for a message with id: {message.Id}");

            await _bot.DeleteMessage(message.Chat.Id, message.Id, cancellationToken);
            _logger.LogInformation(
                $"Deleted user message with id: {message.Id} in chat with id: {message.Chat.Id}");

            string newMessageText = message.Text.Replace("/edit", string.Empty, StringComparison.InvariantCulture);

            var correlations = (await _correlationService.FindBySourceIdAsync(replyToMessage.Id.ToString()))
                .ToList();

            if (correlations is [])
            {
                _logger.LogInformation($"No correlations found for message with id: {replyToMessage.Id}");
                return;
            }

            foreach (NoteCorrelationDto correlation in correlations)
            {
                UpdateNote.Response updateResponse = await _noteService.UpdateNodeAsync(
                    new UpdateNote.Request(correlation.NoteId, newMessageText),
                    cancellationToken);

                if (updateResponse is UpdateNote.Response.PersistenceFailure failure)
                {
                    _logger.LogInformation(
                        $"Failed to edit message with id: {replyToMessage.Id}, persistence failure: {failure.Message}");
                    return;
                }

                _logger.LogInformation($"Edited message with id: {replyToMessage.Id}");
            }
        }

        NoteChannelDto? currentChannel = await _channelService.FindBySourceChannelIdAsync(message.Chat.Id.ToString());

        if (currentChannel is null)
        {
            _logger.LogInformation(
                $"Message was skipped, as a telegram chat with id {message.Chat.Id} is not registered...");
            return;
        }

        CreateNote.Response createResponse = await _noteService.CreateNoteAsync(
            new CreateNote.Request(messageText, currentChannel.NoteNamespaceId),
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

    private async Task RegisterChannelAsync(
        Message message,
        long? noteNamespaceId,
        CancellationToken cancellationToken)
    {
        RegisterChannel.Response response = await _channelService.RegisterChannelAsync(
            new RegisterChannel.Request(message.Chat.Id.ToString(), noteNamespaceId),
            cancellationToken);

        switch (response)
        {
            case RegisterChannel.Response.PersistenceFailure persistenceFailure:
                _logger.LogError(
                    $"Failed to register the chat with id: {message.Chat.Id}, error: {persistenceFailure.Message}");
                break;
            case RegisterChannel.Response.Success success:
                _logger.LogInformation(
                    $"Successfully registered the chat with id: {message.Chat.Id}");

                var notes = (await _noteService.GetAllByNamespaceId(success.Channel.NoteNamespaceId))
                    .ToList();

                foreach (NoteDto note in notes)
                {
                    await SendMessageAsync(success.Channel, note);
                }

                break;
        }
    }

    private async Task UnregisterChannelAsync(Message message, CancellationToken cancellationToken)
    {
        UnregisterChannel.Response response = await _channelService.UnregisterChannelAsync(
            new UnregisterChannel.Request(message.Chat.Id.ToString()),
            cancellationToken);

        switch (response)
        {
            case UnregisterChannel.Response.PersistenceFailure persistenceFailure:
                _logger.LogError(
                    $"Failed to unregister the chat with id: {message.Chat.Id}, error: {persistenceFailure.Message}");
                break;

            case UnregisterChannel.Response.Success:
                _logger.LogInformation($"Successfully unregistered the chat with id: {message.Chat.Id}");
                break;
        }
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
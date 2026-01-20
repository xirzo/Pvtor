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
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.MessageHandling;

public class NoteSyncer : INoteChangedSubscriber
{
    private readonly INoteCorrelationService _correlationService;
    private readonly INoteChannelService _channelService;
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<NoteSyncer> _logger;

    public NoteSyncer(
        INoteCorrelationService correlationService,
        INoteChannelService channelService,
        ITelegramBotClient bot,
        ILogger<NoteSyncer> logger)
    {
        _correlationService = correlationService;
        _channelService = channelService;
        _bot = bot;
        _logger = logger;
    }

    public async Task OnNoteChanged(NoteDto note)
    {
        _logger.LogInformation($"Note with id {note.NoteId} was changed, syncing telegram...");

        IEnumerable<NoteCorrelationDto> correlations = await _correlationService.FindByNoteIdAsync(note.NoteId);
        var correlationMap = correlations.ToDictionary(x => x.NoteChannelId);

        // TODO: find channels by namespace id
        IEnumerable<NoteChannelDto> allChats = await _channelService.GetAllAsync();

        foreach (NoteChannelDto chat in allChats)
        {
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
}
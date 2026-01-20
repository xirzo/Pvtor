using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.MessageHandling;

public class NoteCreator
{
    private readonly INoteService _noteService;
    private readonly INoteChannelService _channelService;
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<NoteCreator> _logger;

    public NoteCreator(
        INoteService noteService,
        INoteChannelService channelService,
        ITelegramBotClient bot,
        ILogger<NoteCreator> logger)
    {
        _noteService = noteService;
        _channelService = channelService;
        _bot = bot;
        _logger = logger;
    }

    public async Task CreateFromMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        NoteChannelDto? currentChannel = await _channelService.FindBySourceChannelIdAsync(message.Chat.Id.ToString());

        if (currentChannel is null)
        {
            _logger.LogInformation(
                $"Message was skipped, as a telegram chat with id {message.Chat.Id} is not registered...");
            return;
        }

        if (message.Text is not { } messageText)
        {
            _logger.LogError(
                $"Cannot create a not for message with id: {message.Id}, as it has no text");
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
}
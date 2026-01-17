using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot;

public class BotUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<BotUpdateHandler> _logger;
    private readonly INoteService _noteService;

    public BotUpdateHandler(ITelegramBotClient bot, ILogger<BotUpdateHandler> logger, INoteService noteService)
    {
        _bot = bot;
        _logger = logger;
        _noteService = noteService;
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

        CreateNote.Response createNoteResponse =
            await _noteService.CreateNoteAsync(new CreateNote.Request(messageText), cancellationToken);

        switch (createNoteResponse)
        {
            case CreateNote.Response.PersistenceFailure persistenceFailure:
                _logger.LogInformation(
                    $"Failed to save message with id: {message.Id}, persistence failure: {persistenceFailure.Message}");
                break;
            case CreateNote.Response.Success success:
                _logger.LogInformation($"Saved message with id: {message.Id} as note with id: {success.Note.NoteId}");
                break;
        }

        await _bot.SendMessage(message.Chat, "DEBUG: Saved message", cancellationToken: cancellationToken);
    }

    private async Task OnMessageEdited(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive edited message, type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        CreateNote.Response createNoteResponse =
            await _noteService.CreateNoteAsync(new CreateNote.Request(messageText), cancellationToken);

        switch (createNoteResponse)
        {
            case CreateNote.Response.PersistenceFailure persistenceFailure:
                _logger.LogInformation(
                    $"Failed to save message with id: {message.Id}, persistence failure: {persistenceFailure.Message}");
                break;
            case CreateNote.Response.Success success:
                _logger.LogInformation($"Saved message with id: {message.Id} as note with id: {success.Note.NoteId}");
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
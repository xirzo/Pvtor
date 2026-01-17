using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Contracts.Notes.Models;
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
    private readonly INoteCorrelationService _correlationService;

    public BotUpdateHandler(
        ITelegramBotClient bot,
        ILogger<BotUpdateHandler> logger,
        INoteService noteService,
        INoteCorrelationService correlationService)
    {
        _bot = bot;
        _logger = logger;
        _noteService = noteService;
        _correlationService = correlationService;
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
                _logger.LogInformation(
                    $"Saved message with id: {message.Id} as note with id: {createSuccess.Note.NoteId}");

                RecordCorrelation.Response recordResponse =
                    await _correlationService.RecordCorrelationAsync(
                        new RecordCorrelation.Request(createSuccess.Note.NoteId, message.Id.ToString()),
                        cancellationToken);

                switch (recordResponse)
                {
                    case RecordCorrelation.Response.PersistenceFailure recordPersistenceFailure:
                        _logger.LogError(
                            $"Failed to record correlation for message with id: {message.Id}, persistence failure: {recordPersistenceFailure.Message}");
                        break;
                    case RecordCorrelation.Response.Success:
                        _logger.LogInformation(
                            $"Saved correlation for message with id: {message.Id} (note with id: {createSuccess.Note.NoteId})");
                        break;
                }

                break;
        }

        await _bot.SendMessage(message.Chat, "DEBUG: Saved message", cancellationToken: cancellationToken);
    }

    private async Task OnMessageEdited(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive edited message, type: {MessageType}", message.Type);
        if (message.Text is not { } newMessageText)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        NoteCorrelationDto? correlation =
            await _correlationService.FindBySourceIdAsync(message.Id.ToString());

        if (correlation is null)
        {
            _logger.LogInformation("Message with id: {MessageId} doesn't exist", message.Id);
            await OnMessage(message, cancellationToken);
            return;
        }

        UpdateNote.Response updateNoteResponse =
            await _noteService.UpdateNodeAsync(
                new UpdateNote.Request(correlation.NoteId, newMessageText),
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
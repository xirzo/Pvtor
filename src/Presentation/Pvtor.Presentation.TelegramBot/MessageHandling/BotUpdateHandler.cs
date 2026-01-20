using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.MessageHandling;

public class BotUpdateHandler : IUpdateHandler
{
    private readonly ILogger<BotUpdateHandler> _logger;
    private readonly MessageHandler _handler;

    public BotUpdateHandler(
        ILogger<BotUpdateHandler> logger,
        MessageHandler handler,
        INoteChangedSubscriber noteChangedSubscriber,
        INoteService noteService)
    {
        _logger = logger;
        _handler = handler;
        noteService.AddSubscriber(noteChangedSubscriber);
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
            { ChannelPost: { } message } => _handler.HandleAsync(message, cancellationToken),
            { Message: { } message } => _handler.HandleAsync(message, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update),
        });
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation($"Unknown update type: {update.Type}");
        return Task.CompletedTask;
    }
}
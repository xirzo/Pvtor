using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Pvtor.Presentation.TelegramBot;

public class BotUpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _bot;
    private readonly ILogger<BotUpdateHandler> _logger;

    public BotUpdateHandler(ITelegramBotClient bot, ILogger<BotUpdateHandler> logger)
    {
        _bot = bot;
        _logger = logger;
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
            { Message: { } message } => OnMessage(message),
            { EditedMessage: { } message } => OnMessage(message),
            _ => UnknownUpdateHandlerAsync(update),
        });
    }

    private async Task OnMessage(Message msg)
    {
        _logger.LogInformation("Receive message type: {MessageType}", msg.Type);
        if (msg.Text is not { } messageText)
        {
            return;
        }

        Message sentMessage = await (messageText.Split(' ')[0] switch
        {
            _ => Usage(msg),
        });
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.Id);
    }

    private async Task<Message> Usage(Message msg)
    {
        const string usage = """
                                 <b><u>Bot menu</u></b>:
                                 /photo          - send a photo
                                 /inline_buttons - send inline buttons
                                 /keyboard       - send keyboard buttons
                                 /remove         - remove keyboard buttons
                                 /request        - request location or contact
                                 /inline_mode    - send inline-mode results list
                                 /poll           - send a poll
                                 /poll_anonymous - send an anonymous poll
                                 /throw          - what happens if handler fails
                             """;
        return await _bot.SendMessage(
            msg.Chat,
            usage,
            parseMode: ParseMode.Html,
            replyMarkup: new ReplyKeyboardRemove());
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }
}
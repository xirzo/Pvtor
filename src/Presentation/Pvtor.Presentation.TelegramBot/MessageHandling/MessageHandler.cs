using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.MessageHandling;

public class MessageHandler
{
    private readonly ILogger<MessageHandler> _logger;
    private readonly CommandProcessor _commandProcessor;
    private readonly ITelegramBotClient _bot;
    private readonly NoteCreator _noteCreator;

    public MessageHandler(
        ILogger<MessageHandler> logger,
        CommandProcessor commandProcessor,
        ITelegramBotClient bot,
        NoteCreator noteCreator)
    {
        _logger = logger;
        _commandProcessor = commandProcessor;
        _bot = bot;
        _noteCreator = noteCreator;
    }

    public async Task HandleAsync(Message message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Received message with type: {message.Type}, message id: {message.Id}");

        if (message.Text is null)
        {
            _logger.LogInformation("Message text is null, skipping...");
            return;
        }

        if (message.Text.StartsWith('/'))
        {
            await _bot.DeleteMessage(message.Chat.Id, message.Id, cancellationToken);
            _logger.LogInformation(
                $"Deleted user message with id: {message.Id} in chat with id: {message.Chat.Id}");
            await _commandProcessor.TryExecuteAsync(message, cancellationToken);
            return;
        }

        await _noteCreator.CreateFromMessageAsync(message, cancellationToken);
    }
}
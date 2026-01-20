using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Presentation.TelegramBot.Commands;
using Pvtor.Presentation.TelegramBot.Parsing;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.MessageHandling;

public class CommandProcessor
{
    private readonly ArgParser _argParser;
    private readonly ILogger<CommandProcessor> _logger;
    private readonly ITelegramBotClient _bot;
    private readonly INoteService _noteService;
    private readonly INoteCorrelationService _correlationService;
    private readonly INoteChannelService _channelService;
    private readonly INoteNamespaceService _namespaceService;

    public CommandProcessor(
        ArgParser argParser,
        ILogger<CommandProcessor> logger,
        ITelegramBotClient bot,
        INoteService noteService,
        INoteCorrelationService correlationService,
        INoteChannelService channelService,
        INoteNamespaceService namespaceService)
    {
        _argParser = argParser;
        _logger = logger;
        _bot = bot;
        _noteService = noteService;
        _correlationService = correlationService;
        _channelService = channelService;
        _namespaceService = namespaceService;
    }

    public async Task<bool> TryExecuteAsync(Message message, CancellationToken cancellationToken = default)
    {
        if (message.Text is not { } messageText)
        {
            _logger.LogError($"Not able to parse command from message with empty text, id: {message.Id}");
            return false;
        }

        _logger.LogInformation(
            $"Parsing message with text: {messageText}");
        ParseResult parseResult = _argParser.Parse(messageText);

        if (parseResult is ParseResult.Failure parseFailure)
        {
            _logger.LogError($"Failed to parse command: {parseFailure.Error.ToString()}");
            return false;
        }

        if (parseResult is ParseResult.Success parseSuccess)
        {
            _logger.LogInformation("Successfully parsed command, executing...");
            var context = new CommandExecuteContext(
                message,
                _bot,
                _namespaceService,
                _channelService,
                _noteService,
                _correlationService,
                _logger);
            await parseSuccess.Command.ExecuteAsync(context, cancellationToken);
            return true;
        }

        throw new UnreachableException();
    }
}
using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.Commands;

public sealed record CommandExecuteContext(
    Message Message,
    ITelegramBotClient Bot,
    INoteNamespaceService NamespaceService,
    INoteChannelService ChannelService,
    INoteService NoteService,
    INoteCorrelationService CorrelationService,
    ILogger Logger);
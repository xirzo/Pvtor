using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Mark.Hidden;

public class MarkHiddenCommand : ICommand
{
    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        context.Logger.LogInformation($"Receive mark command for a message with id: {context.Message.Id}");

        if (context.Message.ReplyToMessage is not { } replyToMessage)
        {
            context.Logger.LogError($"Cannot mark hidden a message without a reply");
            await context.Bot.SendMessage(
                context.Message.Chat.Id,
                "Cannot mark hidden a Message without a reply",
                cancellationToken: cancellationToken);
            return;
        }

        // NOTE: this works only because note_source_id has dependency on part of primary key (low normal form)
        NoteCorrelationDto? correlation =
            (await context.CorrelationService.FindBySourceIdAsync(replyToMessage.Id.ToString()))
            .FirstOrDefault();

        if (correlation is null)
        {
            context.Logger.LogError($"Cannot mark hidden as reply message does not have correlation to any chats");
            await context.Bot.SendMessage(
                context.Message.Chat.Id,
                "Cannot mark hidden as reply message does not have correlation to any chats",
                cancellationToken: cancellationToken);
            return;
        }

        MarkNoteAsHidden.Response markResponse =
            await context.NoteService.MarkNoteAsHidden(
                new MarkNoteAsHidden.Request(correlation.NoteId),
                cancellationToken);

        switch (markResponse)
        {
            case MarkNoteAsHidden.Response.NotFound notFound:
                context.Logger.LogError(notFound.Message);
                break;
            case MarkNoteAsHidden.Response.PersistenceFailure persistenceFailure:
                context.Logger.LogError(
                    $"Failed to mark message hidden with id: {context.Message.Id}, persistence error: {persistenceFailure.Message}");
                break;
            case MarkNoteAsHidden.Response.Success:
                context.Logger.LogInformation(
                    $"Successfully marked message hidden with id: {context.Message.Id}");
                break;
        }
    }
}
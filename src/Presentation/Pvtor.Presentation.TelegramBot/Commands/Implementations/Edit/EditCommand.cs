using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Edit;

public class EditCommand : ICommand
{
    private readonly string _content;

    public EditCommand(string content)
    {
        _content = content;
    }

    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        context.Logger.LogInformation($"Receive edit command for a context.Message with id: {context.Message.Id}");

        if (context.Message.ReplyToMessage is not { } replyToMessage)
        {
            context.Logger.LogError($"Cannot edit a message without a reply");
            await context.Bot.SendMessage(
                context.Message.Chat.Id,
                "Cannot edit a Message without a reply",
                cancellationToken: cancellationToken);
            return;
        }

        context.Logger.LogInformation($"New edited text: \"{_content}\"");

        var correlations = (await context.CorrelationService.FindBySourceIdAsync(replyToMessage.Id.ToString()))
            .ToList();

        if (correlations is [])
        {
            context.Logger.LogInformation($"No correlations found for context.Message with id: {replyToMessage.Id}");
            return;
        }

        foreach (NoteCorrelationDto correlation in correlations)
        {
            UpdateNote.Response updateResponse = await context.NoteService.UpdateNoteAsync(
                new UpdateNote.Request(correlation.NoteId, _content),
                cancellationToken);

            if (updateResponse is UpdateNote.Response.PersistenceFailure failure)
            {
                context.Logger.LogInformation(
                    $"Failed to edit context.Message with id: {replyToMessage.Id}, persistence failure: {failure.Message}");
                return;
            }

            context.Logger.LogInformation($"Edited context.Message with id: {replyToMessage.Id}");
        }
    }
}
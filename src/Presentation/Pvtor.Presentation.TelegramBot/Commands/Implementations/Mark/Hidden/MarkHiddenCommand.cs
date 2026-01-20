using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Mark.Hidden;

public class MarkHiddenCommand : ICommand
{
    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        MarkNoteAsHidden.Response markResponse =
            await context.NoteService.MarkNoteAsHidden(
                new MarkNoteAsHidden.Request(context.Message.Id),
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
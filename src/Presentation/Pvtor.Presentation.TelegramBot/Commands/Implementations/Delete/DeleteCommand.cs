using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Delete;

public class DeleteCommand : ICommand
{
    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        if (context.Message.ReplyToMessage is null)
        {
            return;
        }

        var correlations =
            (await context.CorrelationService.FindBySourceIdAsync(context.Message.ReplyToMessage.Id.ToString()))
            .ToList();

        DeleteNote.Response noteResponse =
            await context.NoteService.DeleteAsync(correlations[0].NoteId, cancellationToken);

        switch (noteResponse)
        {
            case DeleteNote.Response.PersistenceFailure persistenceFailure:
                context.Logger.LogError(
                    "Delete persistence failure for note ID {NoteId}: {ErrorMessage}",
                    correlations[0].NoteId,
                    persistenceFailure.Message);
                break;
            case DeleteNote.Response.Success:
                context.Logger.LogInformation(
                    "Successfully deleted note ID {NoteId}",
                    correlations[0].NoteId);
                break;
        }

        foreach (NoteCorrelationDto correlation in correlations)
        {
            try
            {
                await context.Bot.DeleteMessage(context.Message.Chat.Id, context.Message.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(
                    "Failed to delete message {MessageId} in chat {ChatId}: {ErrorMessage}",
                    context.Message.Id,
                    context.Message.Chat.Id,
                    ex.Message);
                continue;
            }

            // NOTE: maybe not needed as correlations is cascade deleted on note deletion
            DeleteCorrelation.Response correlationResponse = await context.CorrelationService.DeleteAsync(
                new DeleteCorrelation.Request(correlation.NoteSourceId, correlation.NoteChannelId),
                cancellationToken);

            switch (correlationResponse)
            {
                case DeleteCorrelation.Response.PersistenceFailure persistenceFailure:
                    context.Logger.LogError(
                        "Delete persistence failure for note source ID {NoteSourceId} and channel ID {NoteChannelId}: {ErrorMessage}",
                        correlation.NoteSourceId,
                        correlation.NoteChannelId,
                        persistenceFailure.Message);
                    break;
                case DeleteCorrelation.Response.Success:
                    context.Logger.LogInformation(
                        "Successfully deleted correlation for note source ID {NoteSourceId} and channel ID {NoteChannelId}",
                        correlation.NoteSourceId,
                        correlation.NoteChannelId);
                    break;
            }
        }
    }
}
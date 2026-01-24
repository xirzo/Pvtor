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
        if (context.Message.ReplyToMessage is not { } replyToMessage)
        {
            return;
        }

        var correlations =
            (await context.CorrelationService.FindBySourceIdAsync(replyToMessage.Id.ToString()))
            .ToList();

        var noteId = correlations[0].NoteId;

        foreach (NoteCorrelationDto correlation in correlations)
        {
            try
            {
                var noteSourceId = Convert.ToInt32(correlation.NoteSourceId);
                await context.Bot.DeleteMessage(correlation.NoteChannelId, noteSourceId, cancellationToken);
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

        DeleteNote.Response noteResponse =
            await context.NoteService.DeleteAsync(noteId, cancellationToken);

        switch (noteResponse)
        {
            case DeleteNote.Response.PersistenceFailure persistenceFailure:
                context.Logger.LogError(
                    "Delete persistence failure for note ID {NoteId}: {ErrorMessage}",
                    noteId,
                    persistenceFailure.Message);
                break;
            case DeleteNote.Response.Success:
                context.Logger.LogInformation(
                    "Successfully deleted note ID {NoteId}",
                    noteId);
                break;
        }
    }
}
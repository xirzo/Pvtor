using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System;
using System.Collections.Generic;
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

        // FIX: this actually always returns one message (reply to message will always have one correlation:
        // correlation to the chat it is in
        NoteCorrelationDto replyMessageCorrelation =
            (await context.CorrelationService.FindBySourceIdAsync(replyToMessage.Id.ToString()))
            .Single();

        long noteId = replyMessageCorrelation.NoteId;

        IEnumerable<NoteCorrelationDto> correlations = await context.CorrelationService.FindByNoteIdAsync(noteId);

        foreach (NoteCorrelationDto correlation in correlations)
        {
            try
            {
                int noteSourceId = Convert.ToInt32(correlation.NoteSourceId);

                // TODO: rewrite with clear method for finding concrete
                NoteChannelDto channel = (await context.ChannelService.GetAllAsync())
                    .Single(x => x.NoteChannelId == correlation.NoteChannelId);

                await context.Bot.DeleteMessage(channel.NoteSourceChannelId, noteSourceId, cancellationToken);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(
                    "Failed to delete message {MessageId} in chat: {ErrorMessage}",
                    correlation.NoteSourceId,
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
using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Models;
using Pvtor.Application.Contracts.Notes.Operations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Register;

public class RegisterCommand : ICommand
{
    private readonly string? _noteNamespaceName;

    public RegisterCommand(string? noteNamespaceName)
    {
        _noteNamespaceName = noteNamespaceName;
    }

    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        // await context.Bot.DeleteMessage(context.Message.Chat.Id, context.Message.Id, cancellationToken);
        //
        // context.Logger.LogInformation(
        //     $"Deleted user message with id: {context.Message.Id} in chat with id: {context.Message.Chat.Id}");
        if (_noteNamespaceName is null)
        {
            context.Logger.LogInformation("Register command did not provide a namespace, using default...");
            await RegisterChannelAsync(context, context.Message, null, cancellationToken);
            return;
        }

        NoteNamespaceDto? noteNamespace = await context.NamespaceService.FindByNameAsync(_noteNamespaceName);

        if (noteNamespace is null)
        {
            context.Logger.LogInformation(
                $"Namespace with name: {_noteNamespaceName} does not exist, creating a new...");
            CreateNamespace.Response response =
                await context.NamespaceService.CreateAsync(new CreateNamespace.Request(_noteNamespaceName));

            switch (response)
            {
                case CreateNamespace.Response.PersistenceFailure persistenceFailure:
                    context.Logger.LogError(
                        $"Failed to create a namespace with name: {_noteNamespaceName}, error: {persistenceFailure.Message}");
                    return;
                case CreateNamespace.Response.Success success:
                    noteNamespace = success.Namespace;
                    context.Logger.LogInformation(
                        $"Successfully created a new namespace with name: {_noteNamespaceName}");
                    break;
            }
        }

        await RegisterChannelAsync(context, context.Message, noteNamespace?.NoteNamespaceId, cancellationToken);
    }

    private async Task RegisterChannelAsync(
        CommandExecuteContext context,
        Message message,
        long? noteNamespaceId,
        CancellationToken cancellationToken)
    {
        RegisterChannel.Response response = await context.ChannelService.RegisterChannelAsync(
            new RegisterChannel.Request(message.Chat.Id.ToString(), noteNamespaceId),
            cancellationToken);

        switch (response)
        {
            case RegisterChannel.Response.PersistenceFailure persistenceFailure:
                context.Logger.LogError(
                    $"Failed to register the chat with id: {message.Chat.Id}, error: {persistenceFailure.Message}");
                break;
            case RegisterChannel.Response.Success success:
                context.Logger.LogInformation(
                    $"Successfully registered the chat with id: {message.Chat.Id}");

                var notes = (await context.NoteService.GetAllByNamespaceId(success.Channel.NoteNamespaceId))
                    .ToList();

                foreach (NoteDto note in notes)
                {
                    await SendMessageAsync(context, success.Channel, note);
                }

                break;
        }
    }

    private async Task SendMessageAsync(CommandExecuteContext context, NoteChannelDto chat, NoteDto note)
    {
        try
        {
            Message message = await context.Bot.SendMessage(new ChatId(chat.NoteSourceChannelId), note.Content);
            context.Logger.LogInformation($"Sent message to chat with id: {chat.NoteSourceChannelId}");

            await RecordCorrelation(context, message, note.NoteId, chat.NoteChannelId);
        }
        catch (Exception ex)
        {
            context.Logger.LogError(ex, $"Failed to send/record message in chat {chat.NoteSourceChannelId}");
        }
    }

    private async Task RecordCorrelation(
        CommandExecuteContext context,
        Message message,
        long noteId,
        long channelId,
        CancellationToken cancellationToken = default)
    {
        context.Logger.LogInformation($"Saved message with id: {message.Id} as note with id: {noteId}");

        RecordCorrelation.Response response = await context.CorrelationService.RecordCorrelationAsync(
            new RecordCorrelation.Request(noteId, message.Id.ToString(), channelId),
            cancellationToken);

        if (response is RecordCorrelation.Response.PersistenceFailure failure)
        {
            context.Logger.LogError(
                $"Failed to record correlation for message with id: {message.Id}, persistence failure: {failure.Message}");
        }
        else
        {
            context.Logger.LogInformation(
                $"Saved correlation for message with id: {message.Id} (note with id: {noteId})");
        }
    }
}
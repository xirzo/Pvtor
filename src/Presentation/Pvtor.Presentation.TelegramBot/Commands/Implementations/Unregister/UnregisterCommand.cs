using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Operations;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Unregister;

public class UnregisterCommand : ICommand
{
    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        await context.Bot.DeleteMessage(context.Message.Chat.Id, context.Message.Id, cancellationToken);
        context.Logger.LogInformation(
            $"Deleted user message with id: {context.Message.Id} in chat with id: {context.Message.Chat.Id}");

        UnregisterChannel.Response response = await context.ChannelService.UnregisterChannelAsync(
            new UnregisterChannel.Request(context.Message.Chat.Id.ToString()),
            cancellationToken);

        switch (response)
        {
            case UnregisterChannel.Response.PersistenceFailure persistenceFailure:
                context.Logger.LogError(
                    $"Failed to unregister the chat with id: {context.Message.Chat.Id}, error: {persistenceFailure.Message}");
                break;

            case UnregisterChannel.Response.Success:
                context.Logger.LogInformation($"Successfully unregistered the chat with id: {context.Message.Chat.Id}");
                break;
        }
    }
}
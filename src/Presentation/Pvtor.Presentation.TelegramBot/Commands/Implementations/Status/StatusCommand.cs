using Microsoft.Extensions.Logging;
using Pvtor.Application.Contracts.Notes.Models;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Status;

public class StatusCommand : ICommand
{
    public async Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        context.Logger.LogInformation($"Executing status command for chat {context.Message.Chat.Id}");

        var builder = new StringBuilder();

        NoteChannelDto? channel =
            await context.ChannelService.FindBySourceChannelIdAsync(context.Message.Chat.Id.ToString());

        if (channel is null)
        {
            builder.AppendLine("Chat is not connected to Pvtor.");
        }
        else
        {
            builder.AppendLine("Chat is connected to Pvtor.");
            builder.AppendLine($"- Chat ID (Channel Source ID): {context.Message.Chat.Id}");
            builder.AppendLine($"- Channel ID: {channel.NoteChannelId}");
        }

        await context.Bot.SendMessage(
            context.Message.Chat.Id,
            builder.ToString(),
            cancellationToken: cancellationToken);
    }
}
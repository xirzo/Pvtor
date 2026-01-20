using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Mark.Hidden;

public class MarkHiddenCommand : ICommand
{
    public Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default)
    {
        // It should mark note as "read"
        throw new System.NotImplementedException();
    }
}
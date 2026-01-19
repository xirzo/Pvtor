using System.Threading;
using System.Threading.Tasks;

namespace Pvtor.Presentation.TelegramBot.Commands;

public interface ICommand
{
    Task ExecuteAsync(CommandExecuteContext context, CancellationToken cancellationToken = default);
}
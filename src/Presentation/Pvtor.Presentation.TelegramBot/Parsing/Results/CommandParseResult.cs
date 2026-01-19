using Pvtor.Presentation.TelegramBot.Commands;
using Pvtor.Presentation.TelegramBot.Errors;

namespace Pvtor.Presentation.TelegramBot.Parsing.Results;

public abstract record CommandParseResult
{
    private CommandParseResult() { }

    public sealed record Success(ICommand Command) : CommandParseResult;

    public sealed record Failure(IParseError Error) : CommandParseResult;
}
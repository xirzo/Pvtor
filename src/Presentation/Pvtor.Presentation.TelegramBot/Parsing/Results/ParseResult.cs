using Pvtor.Presentation.TelegramBot.Commands;
using Pvtor.Presentation.TelegramBot.Errors;

namespace Pvtor.Presentation.TelegramBot.Parsing.Results;

public abstract record ParseResult
{
    private ParseResult() { }

    public sealed record Success(ICommand Command) : ParseResult;

    public sealed record Failure(IParseError Error) : ParseResult;
}
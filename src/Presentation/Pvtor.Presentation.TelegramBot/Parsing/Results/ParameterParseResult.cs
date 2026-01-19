using Pvtor.Presentation.TelegramBot.Errors;

namespace Pvtor.Presentation.TelegramBot.Parsing.Results;

public abstract record ParameterParseResult
{
    private ParameterParseResult() { }

    public sealed record Success : ParameterParseResult;

    public sealed record Failure(IParseError Error) : ParameterParseResult;
}
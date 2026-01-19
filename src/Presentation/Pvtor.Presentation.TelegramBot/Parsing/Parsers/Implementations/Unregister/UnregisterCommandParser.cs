using Pvtor.Presentation.TelegramBot.Commands.Implementations.Unregister;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Unregister;

public class UnregisterCommandParser : CommandParserBase
{
    // private readonly IParameterParser<UnregisterCommandBuilder> _parametersParser;
    //
    // public EditCommandParser(IParameterParser<UnregisterCommandBuilder> parametersParser)
    // {
    //     _parametersParser = parametersParser;
    // }
    public override CommandParseResult Parse(IEnumerator<string> words)
    {
        if (words.Current is not "/unregister")
        {
            if (Next is null)
            {
                return new CommandParseResult.Failure(new NoCommandParserError(words.Current));
            }

            return Next.Parse(words);
        }

        words.MoveNext();

        var builder = new UnregisterCommandBuilder();

        // ParameterParseResult parameterParseResult = _parametersParser.Parse(words, builder);
        //
        // if (parameterParseResult is ParameterParseResult.Failure failure)
        // {
        //     return new CommandParseResult.Failure(failure.Error);
        // }
        return new CommandParseResult.Success(builder.Build());
    }
}
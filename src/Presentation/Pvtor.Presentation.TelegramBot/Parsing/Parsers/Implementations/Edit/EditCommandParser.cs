using Pvtor.Presentation.TelegramBot.Commands.Implementations.Edit;
using Pvtor.Presentation.TelegramBot.Errors;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Edit;

public class EditCommandParser : CommandParserBase
{
    private readonly IParameterParser<EditCommandBuilder> _parametersParser;

    public EditCommandParser(IParameterParser<EditCommandBuilder> parametersParser)
    {
        _parametersParser = parametersParser;
    }

    public override CommandParseResult Parse(IEnumerator<string> words)
    {
        if (words.Current is not "/edit")
        {
            if (Next is null)
            {
                return new CommandParseResult.Failure(new NoCommandParserError(words.Current));
            }

            return Next.Parse(words);
        }

        words.MoveNext();

        var builder = new EditCommandBuilder();

        ParameterParseResult parameterParseResult = _parametersParser.Parse(words, builder);

        if (parameterParseResult is ParameterParseResult.Failure failure)
        {
            return new CommandParseResult.Failure(failure.Error);
        }

        return new CommandParseResult.Success(builder.Build());
    }
}
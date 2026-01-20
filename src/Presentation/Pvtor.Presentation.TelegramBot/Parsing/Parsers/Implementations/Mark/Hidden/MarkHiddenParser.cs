using Pvtor.Presentation.TelegramBot.Commands.Implementations.Mark.Hidden;
using Pvtor.Presentation.TelegramBot.Errors;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Mark.Hidden;

public class MarkHiddenParser : CommandParserBase
{
    public override CommandParseResult Parse(IEnumerator<string> words)
    {
        if (words.Current is not "hidden")
        {
            if (Next is null)
            {
                return new CommandParseResult.Failure(new NoCommandParserError(words.Current));
            }

            return Next.Parse(words);
        }

        words.MoveNext();

        var builder = new MarkHiddenCommandBuilder();

        return new CommandParseResult.Success(builder.Build());
    }
}
using Pvtor.Presentation.TelegramBot.Commands.Implementations.Status;
using Pvtor.Presentation.TelegramBot.Errors.Implementations;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Status;

public class StatusCommandParser : CommandParserBase
{
    public override CommandParseResult Parse(IEnumerator<string> words)
    {
        if (words.Current is not "/status")
        {
            if (Next is null)
            {
                return new CommandParseResult.Failure(new NoCommandParserError(words.Current));
            }

            return Next.Parse(words);
        }

        words.MoveNext();

        var builder = new StatusCommandBuilder();

        return new CommandParseResult.Success(builder.Build());
    }
}
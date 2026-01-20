using Pvtor.Presentation.TelegramBot.Errors;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Mark;

public class MarkParser : CommandParserBase
{
    private readonly ICommandParser _markParser;

    public MarkParser(ICommandParser markParser)
    {
        _markParser = markParser;
    }

    public override CommandParseResult Parse(IEnumerator<string> words)
    {
        if (words.Current is not "mark")
        {
            if (Next is null)
            {
                return new CommandParseResult.Failure(new NoCommandParserError(words.Current));
            }

            return Next.Parse(words);
        }

        words.MoveNext();

        return _markParser.Parse(words);
    }
}
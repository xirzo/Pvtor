using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers;

public abstract class CommandParserBase : ICommandParser
{
    protected ICommandParser? Next { get; private set; }

    public ICommandParser AddNext(ICommandParser parser)
    {
        if (Next is null)
        {
            Next = parser;
        }
        else
        {
            Next.AddNext(parser);
        }

        return this;
    }

    public abstract CommandParseResult Parse(IEnumerator<string> words);
}
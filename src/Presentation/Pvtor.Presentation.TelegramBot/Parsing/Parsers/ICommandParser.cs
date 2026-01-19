using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers;

public interface ICommandParser
{
    ICommandParser AddNext(ICommandParser parser);

    CommandParseResult Parse(IEnumerator<string> words);
}
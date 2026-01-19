using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers;

public interface IParameterParser<TBuilder>
{
    IParameterParser<TBuilder> AddNext(IParameterParser<TBuilder> parser);

    ParameterParseResult Parse(IEnumerator<string> words, TBuilder builder);
}
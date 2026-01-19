using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers;

public abstract class ParameterParserBase<TBuilder> : IParameterParser<TBuilder>
{
    protected IParameterParser<TBuilder>? Next { get; private set; }

    public IParameterParser<TBuilder> AddNext(IParameterParser<TBuilder> parser)
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

    public abstract ParameterParseResult Parse(IEnumerator<string> words, TBuilder builder);
}
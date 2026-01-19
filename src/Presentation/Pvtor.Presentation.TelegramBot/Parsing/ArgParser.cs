using Pvtor.Presentation.TelegramBot.Parsing.Parsers;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pvtor.Presentation.TelegramBot.Parsing;

public class ArgParser
{
    private readonly ICommandParser _parser;

    public ArgParser(ICommandParser parser)
    {
        _parser = parser;
    }

    public ParseResult Parse(string input)
    {
        List<string> args = input
            .Trim()
            .Split(' ')
            .ToList();

        List<string>.Enumerator iterator = args.GetEnumerator();
        iterator.MoveNext();

        CommandParseResult commandParseResult = _parser.Parse(iterator);

        return commandParseResult switch
        {
            CommandParseResult.Success success => new ParseResult.Success(success.Command),
            CommandParseResult.Failure failure => new ParseResult.Failure(failure.Error),
            _ => throw new UnreachableException(),
        };
    }
}
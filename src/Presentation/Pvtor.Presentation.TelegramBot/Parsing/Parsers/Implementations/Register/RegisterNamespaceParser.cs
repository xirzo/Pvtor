using Pvtor.Presentation.TelegramBot.Commands.Implementations.Register;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Register;

public class RegisterNamespaceParser : ParameterParserBase<RegisterCommandBuilder>
{
    public override ParameterParseResult Parse(IEnumerator<string> words, RegisterCommandBuilder builder)
    {
        string noteNamespace = words.Current;

        builder.WithNamespace(noteNamespace);

        if (Next is not null && words.MoveNext())
        {
            return Next.Parse(words, builder);
        }

        return new ParameterParseResult.Success();
    }
}
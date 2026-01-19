using Pvtor.Presentation.TelegramBot.Commands.Implementations.Edit;
using Pvtor.Presentation.TelegramBot.Parsing.Results;
using System.Collections.Generic;
using System.Text;

namespace Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Edit;

public class EditCommandContentParser : ParameterParserBase<EditCommandBuilder>
{
    public override ParameterParseResult Parse(IEnumerator<string> words, EditCommandBuilder builder)
    {
        var stringBuilder = new StringBuilder();

        while (words.MoveNext())
        {
            stringBuilder.Append(words.Current);
            stringBuilder.Append(' ');
        }

        builder.WithContent(stringBuilder.ToString().TrimEnd());

        if (Next is not null && words.MoveNext())
        {
            return Next.Parse(words, builder);
        }

        return new ParameterParseResult.Success();
    }
}
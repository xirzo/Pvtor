using Pvtor.Presentation.TelegramBot.Errors;

namespace Pvtor.Presentation.TelegramBot;

public class NoCommandParserError : IParseError
{
    private readonly string _command;

    public NoCommandParserError(string command)
    {
        _command = command;
    }

    public override string ToString()
    {
        return $"No command parser found for command: \"{_command}\"";
    }
}
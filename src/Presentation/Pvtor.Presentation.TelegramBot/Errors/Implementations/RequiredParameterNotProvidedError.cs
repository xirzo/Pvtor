namespace Pvtor.Presentation.TelegramBot.Errors.Implementations;

public class RequiredParameterNotProvidedError : IParseError
{
    private readonly string _parameterName;

    public RequiredParameterNotProvidedError(string parameterName)
    {
        _parameterName = parameterName;
    }

    public override string ToString()
    {
        return $"Required parameter with name: \"{_parameterName}\" was not provided";
    }
}
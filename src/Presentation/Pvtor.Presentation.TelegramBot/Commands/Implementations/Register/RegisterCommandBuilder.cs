namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Register;

public class RegisterCommandBuilder
{
    public string? NoteNamespace { get; private set; }

    public RegisterCommandBuilder WithNamespace(string noteNamespace)
    {
        NoteNamespace = noteNamespace;
        return this;
    }

    public RegisterCommand Build()
    {
        return new RegisterCommand(NoteNamespace);
    }
}
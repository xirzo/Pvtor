using System;

namespace Pvtor.Presentation.TelegramBot.Commands.Implementations.Edit;

public class EditCommandBuilder
{
    private string? _content;

    public EditCommandBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public EditCommand Build()
    {
        return new EditCommand(_content ?? throw new ArgumentNullException(nameof(_content)));
    }
}
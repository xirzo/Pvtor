using System;

namespace Pvtor.Presentation.TelegramBot;

public class BotOptions
{
    public string BotToken { get; set; } = null!;

    public Uri BotWebhookUrl { get; set; } = null!;

    public string SecretToken { get; set; } = null!;
}
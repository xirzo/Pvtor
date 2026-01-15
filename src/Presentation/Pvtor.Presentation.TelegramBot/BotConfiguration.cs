using System;

namespace Pvtor.Presentation.TelegramBot;

public record BotConfiguration(string BotToken, Uri BotWebhookUrl, string SecretToken);
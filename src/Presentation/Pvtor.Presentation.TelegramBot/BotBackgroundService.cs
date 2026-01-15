using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot;

public class BotBackgroundService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<BotBackgroundService> _logger;
    private readonly IOptions<BotConfiguration> _config;

    public BotBackgroundService(
        ITelegramBotClient botClient,
        ILogger<BotBackgroundService> logger,
        IOptions<BotConfiguration> config)
    {
        _botClient = botClient;
        _logger = logger;
        _config = config;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Telegram Bot Background Service is stopping.");

        try
        {
            await _botClient.DeleteWebhook(cancellationToken: cancellationToken);
            _logger.LogInformation("Webhook deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete webhook");
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram Bot Background Service started.");

        try
        {
            string webhookUrl = _config.Value.BotWebhookUrl.AbsoluteUri;
            await _botClient.SetWebhook(
                webhookUrl,
                allowedUpdates: [],
                secretToken: _config.Value.SecretToken,
                cancellationToken: stoppingToken);
            _logger.LogInformation("Webhook set to {WebhookUrl}", webhookUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set webhook");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
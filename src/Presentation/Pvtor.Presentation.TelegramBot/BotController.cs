using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pvtor.Presentation.TelegramBot;

[ApiController]
[Route("api/telegram")]
public class BotController : ControllerBase
{
    private readonly IOptions<BotConfiguration> _config;

    public BotController(IOptions<BotConfiguration> config)
    {
        _config = config;
    }

    [HttpGet("webhook")]
    public async Task<string> SetWebhook([FromServices] ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        string webhookUrl = _config.Value.BotWebhookUrl.AbsoluteUri;
        await bot.SetWebhook(
            webhookUrl,
            allowedUpdates: [],
            secretToken: _config.Value.SecretToken,
            cancellationToken: cancellationToken);

        return $"Webhook set to {webhookUrl}";
    }

    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] ITelegramBotClient bot,
        [FromServices] BotUpdateHandler handleUpdateService,
        CancellationToken ct)
    {
        if (Request.Headers["X-Telegram-Bot-Api-Secret-Token"] != _config.Value.SecretToken)
        {
            return Forbid();
        }

        try
        {
            await handleUpdateService.HandleUpdateAsync(bot, update, ct);
        }
        catch (Exception exception)
        {
            await handleUpdateService.HandleErrorAsync(
                bot,
                exception,
                Telegram.Bot.Polling.HandleErrorSource.HandleUpdateError,
                ct);
        }

        return Ok();
    }
}
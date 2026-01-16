using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BotOptions>(configuration.GetRequiredSection("TelegramBot"));

        services.AddControllers();

        services.AddSingleton<ITelegramBotClient>(provider =>
        {
            BotOptions botOptions = provider.GetRequiredService<IOptions<BotOptions>>().Value;

            if (string.IsNullOrWhiteSpace(botOptions.BotToken))
            {
                throw new InvalidOperationException("TelegramBot: BotToken is not configured or is empty.");
            }

            return new TelegramBotClient(botOptions.BotToken);
        });

        services.AddScoped<BotUpdateHandler>();
        services.AddHostedService<BotBackgroundService>();

        return services;
    }
}
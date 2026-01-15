using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Pvtor.Presentation.TelegramBot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSingleton<ITelegramBotClient>(provider =>
        {
            IOptions<BotConfiguration> config = provider.GetRequiredService<IOptions<BotConfiguration>>();
            return new TelegramBotClient(config.Value.BotToken);
        });

        services.AddScoped<BotUpdateHandler>();
        services.AddHostedService<BotBackgroundService>();

        return services;
    }
}
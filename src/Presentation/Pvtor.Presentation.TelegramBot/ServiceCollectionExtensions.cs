using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Presentation.TelegramBot.MessageHandling;
using Pvtor.Presentation.TelegramBot.Parsing;
using Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Edit;
using Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Mark;
using Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Mark.Hidden;
using Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Register;
using Pvtor.Presentation.TelegramBot.Parsing.Parsers.Implementations.Unregister;
using Pvtor.Presentation.TelegramBot.Registration;
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

        var argParser = new ArgParser(new RegisterCommandParser(new RegisterNamespaceParser())
            .AddNext(new UnregisterCommandParser())
            .AddNext(new EditCommandParser(new EditCommandContentParser()))
            .AddNext(new MarkParser(new MarkHiddenParser())));

        services.AddSingleton(argParser);

        services.AddSingleton<MessageHandler>();
        services.AddSingleton<NoteSyncer>();
        services.AddSingleton<CommandProcessor>();
        services.AddSingleton<BotUpdateHandler>();
        services.AddSingleton<NoteCreator>();
        services.AddSingleton<INoteChangedSubscriber, NoteSyncer>();
        services.AddHostedService<BotBackgroundService>();

        return services;
    }
}
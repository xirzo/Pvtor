using Microsoft.Extensions.DependencyInjection;
using Pvtor.Application.Abstractions;

namespace Pvtor.Infrastructure.TelegramRecorder;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramRecorder(this IServiceCollection services)
    {
        services.AddSingleton<INoteCorrelationRecorder, TelegramNoteCorrelationRecorder>();

        return services;
    }
}
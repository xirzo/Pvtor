using Microsoft.Extensions.DependencyInjection;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Services;

namespace Pvtor.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<INoteService, NoteService>();
        services.AddSingleton<INoteCorrelationService, NoteCorrelationService>();
        services.AddSingleton<INoteChannelService, NoteChannelService>();
        services.AddSingleton<INoteNamespaceService, NoteNamespaceService>();
        return services;
    }
}
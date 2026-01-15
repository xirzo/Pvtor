using Microsoft.Extensions.DependencyInjection;
using Pvtor.Application.Contracts.Notes;
using Pvtor.Application.Services;

namespace Pvtor.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<INoteService, NoteService>();
        return services;
    }
}
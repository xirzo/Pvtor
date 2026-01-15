using Microsoft.Extensions.DependencyInjection;

namespace Pvtor.Presentation.Http;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttp(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}
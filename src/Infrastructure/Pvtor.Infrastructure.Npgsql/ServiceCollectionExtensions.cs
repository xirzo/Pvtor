using Microsoft.Extensions.DependencyInjection;
using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Infrastructure.Npgsql.Repositories;

namespace Pvtor.Infrastructure.Npgsql;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNpgsql(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IPersistanceContext, NpgsqlPersistenceContext>();

        services.AddSingleton<INoteRepository>(_ =>
            new NpgsqlNoteRepository(connectionString));
        services.AddSingleton<INoteCorrelationRepository>(_ =>
            new NpgsqlNoteCorrelationRepository(connectionString));
        services.AddSingleton<INoteChannelRepository>(_ =>
            new NpgsqlNoteChannelRepository(connectionString));

        return services;
    }
}
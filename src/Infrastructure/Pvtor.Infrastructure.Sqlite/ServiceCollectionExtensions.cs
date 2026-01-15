using Microsoft.Extensions.DependencyInjection;
using Pvtor.Application.Abstractions.Persistence;
using Pvtor.Application.Abstractions.Persistence.Repositories;
using Pvtor.Infrastructure.Sqlite.Repositories;

namespace Pvtor.Infrastructure.Sqlite;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlite(this IServiceCollection services)
    {
        services.AddSingleton<IPersistanceContext, SqlitePersistenceContext>();
        services.AddSingleton<INoteRepository, SqliteNoteRepository>();
        return services;
    }
}
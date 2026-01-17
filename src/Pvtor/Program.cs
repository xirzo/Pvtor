using Pvtor.Application;
using Pvtor.Infrastructure.Sqlite;
using Pvtor.Presentation.Http;
using Pvtor.Presentation.TelegramBot;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", true)
    .AddEnvironmentVariables();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString is null || string.IsNullOrWhiteSpace(connectionString))
{
    throw new ArgumentNullException(nameof(connectionString), "Provide database connection string");
}

builder.Services
    .AddApplication()
    .AddSqlite(connectionString)
    .AddHttp()
    .AddTelegramBot(builder.Configuration)
    .AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

await app.RunAsync();
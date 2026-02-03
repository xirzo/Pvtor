using Pvtor.Application;
using Pvtor.Infrastructure.Npgsql;
using Pvtor.Presentation.TelegramBot;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (connectionString is null || string.IsNullOrWhiteSpace(connectionString))
{
    throw new ArgumentNullException(nameof(connectionString), "Provide database connection string");
}

builder.Services
    .AddApplication()
    .AddNpgsql(connectionString)
    .AddTelegramBot(builder.Configuration)
    .AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
await app.RunAsync();

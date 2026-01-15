using Pvtor.Application;
using Pvtor.Infrastructure.Sqlite;
using Pvtor.Presentation.Http;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddSqlite()
    .AddHttp()
    .AddOpenApi();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();
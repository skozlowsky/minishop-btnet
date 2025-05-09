using System.Reflection;
using Catalog.Database;
using Catalog.Extensions;
using Catalog.Metrics;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var assembly = Assembly.GetExecutingAssembly();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter("Catalog.Metrics"));

builder.AddNpgsqlDbContext<CatalogContext>("catalogDb");

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(assembly));
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddSingleton<ProductMetrics>();

builder.Services.AddEndpoints(assembly);
builder.Services.AddProblemDetails();
builder.Services.AddCors();

var app = builder.Build();

app.MapEndpoints();

app.MapOpenApi();
app.MapScalarApiReference();

await MigrateUp();

app.UseCors(c => c
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.Run();


async Task MigrateUp()
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();

    await context.Database.MigrateAsync();
}

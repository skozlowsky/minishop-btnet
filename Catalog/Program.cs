using System.Reflection;
using Catalog.Database;
using Catalog.Extensions;
using Catalog.Metrics;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

// TODO check base policy
// builder.Services.AddOutputCache(options =>
// {
//     options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromSeconds(2)));
// });

builder.AddRedisOutputCache("redis");

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapEndpoints();

await MigrateUp();

app.UseCors(c => c
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseOutputCache();

app.Run();


async Task MigrateUp()
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();

    await context.Database.MigrateAsync();
}

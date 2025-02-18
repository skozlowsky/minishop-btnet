using System.Reflection;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Abstractions;
using Order.Database;
using Order.Extensions;
using Order.Services;
using Order.Services.ServiceClients;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var assembly = Assembly.GetExecutingAssembly();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<OrderContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("orderDb")));

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(assembly));
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddInventoryApiClient(builder.Configuration);

builder.Services.AddScoped<IEventPublisher, EventPublisher>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IUserContext, UserContext>();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
            
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitMQ"));
                
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpoints(assembly);

builder.Services.AddCors();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OrderContext>();
    
    await context.Database.MigrateAsync();
}

app.UseCors(c => c
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.Run();
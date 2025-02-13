using MailKit.Net.Smtp;
using MassTransit;
using Notification;
using Notification.Configuration;
using Notification.Services;
using Notification.Services.Abstractions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
// builder.Services.AddSingleton<IEmailService, MockEmailService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderNotificationService>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(builder.Configuration["Services:rabbitmq:amqp:0"]), "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
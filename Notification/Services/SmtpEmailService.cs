using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Notification.Configuration;
using Notification.Services.Abstractions;

namespace Notification.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger, IConfiguration configuration)
    {
        _settings = settings.Value;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_settings.From));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        var smtpUri = new Uri(_configuration["Services:maildev:smtp:0"]);

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpUri.Host, _settings.Port, _settings.UseSsl);
        
        if (!string.IsNullOrEmpty(_settings.Username))
        {
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
        }

        await client.SendAsync(email);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
    }
}
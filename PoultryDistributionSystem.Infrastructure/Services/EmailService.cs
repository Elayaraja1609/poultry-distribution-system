using Microsoft.Extensions.Configuration;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// Email service implementation using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILoggingService _loggingService;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration, ILoggingService loggingService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

        var emailConfig = _configuration.GetSection("Email");
        _smtpServer = emailConfig["SmtpServer"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
        _smtpUsername = emailConfig["SmtpUsername"] ?? string.Empty;
        _smtpPassword = emailConfig["SmtpPassword"] ?? string.Empty;
        _fromEmail = emailConfig["FromEmail"] ?? "noreply@poultrysystem.com";
        _fromName = emailConfig["FromName"] ?? "Poultry Distribution System";
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(to, subject, body, false, cancellationToken);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default)
    {
        try
        {
            // If SMTP is not configured, just log
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                await _loggingService.LogInformationAsync(
                    $"Email would be sent to: {to}, Subject: {subject}, IsHtml: {isHtml}",
                    cancellationToken: cancellationToken);
                return;
            }

            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
            await _loggingService.LogInformationAsync(
                $"Email sent successfully to: {to}, Subject: {subject}",
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            await _loggingService.LogErrorAsync(
                $"Failed to send email to: {to}, Subject: {subject}",
                ex,
                cancellationToken: cancellationToken);
            // Don't throw - email failures shouldn't break the application
        }
    }
}

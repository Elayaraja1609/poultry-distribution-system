using PoultryDistributionSystem.Application.Interfaces;
using InfrastructureEmailService = PoultryDistributionSystem.Infrastructure.Services.Interfaces.IEmailService;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// Adapter to bridge Infrastructure EmailService to Application IEmailService interface
/// </summary>
public class EmailServiceAdapter : IEmailService
{
    private readonly InfrastructureEmailService _emailService;

    public EmailServiceAdapter(InfrastructureEmailService emailService)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        await _emailService.SendEmailAsync(to, subject, body, cancellationToken);
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default)
    {
        await _emailService.SendEmailAsync(to, subject, body, isHtml, cancellationToken);
    }
}

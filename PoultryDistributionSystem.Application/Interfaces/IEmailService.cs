namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Email service interface (Application layer abstraction)
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendEmailAsync(string to, string subject, string body, bool isHtml, CancellationToken cancellationToken = default);
}

namespace PoultryDistributionSystem.Infrastructure.Services.Interfaces;

/// <summary>
/// Logging service interface for file-based logging
/// </summary>
public interface ILoggingService
{
    Task LogErrorAsync(string message, Exception? exception = null, string? requestId = null, CancellationToken cancellationToken = default);
    Task LogWarningAsync(string message, string? requestId = null, CancellationToken cancellationToken = default);
    Task LogInformationAsync(string message, string? requestId = null, CancellationToken cancellationToken = default);
}

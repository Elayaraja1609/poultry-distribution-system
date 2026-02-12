using Microsoft.Extensions.Configuration;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// File-based logging service implementation
/// </summary>
public class LoggingService : ILoggingService
{
    private readonly string _logsFolder;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public LoggingService(IConfiguration configuration)
    {
        var basePath = configuration["Logging:BasePath"] ?? "logs";
        _logsFolder = Path.Combine(Directory.GetCurrentDirectory(), basePath);

        // Ensure logs directory exists
        if (!Directory.Exists(_logsFolder))
        {
            Directory.CreateDirectory(_logsFolder);
        }
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, string? requestId = null, CancellationToken cancellationToken = default)
    {
        var logEntry = CreateLogEntry("ERROR", message, exception, requestId);
        await WriteToFileAsync("error", logEntry, cancellationToken);
    }

    public async Task LogWarningAsync(string message, string? requestId = null, CancellationToken cancellationToken = default)
    {
        var logEntry = CreateLogEntry("WARNING", message, null, requestId);
        await WriteToFileAsync("warning", logEntry, cancellationToken);
    }

    public async Task LogInformationAsync(string message, string? requestId = null, CancellationToken cancellationToken = default)
    {
        var logEntry = CreateLogEntry("INFO", message, null, requestId);
        await WriteToFileAsync("info", logEntry, cancellationToken);
    }

    private string CreateLogEntry(string level, string message, Exception? exception, string? requestId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var requestIdPart = string.IsNullOrEmpty(requestId) ? "" : $" | RequestId: {requestId}";
        var exceptionPart = exception != null ? $" | Exception: {exception.Message}\nStack Trace: {exception.StackTrace}" : "";

        return $"[{timestamp}] [{level}]{requestIdPart} | {message}{exceptionPart}\n";
    }

    private async Task WriteToFileAsync(string logType, string logEntry, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var fileName = $"{logType}_{DateTime.UtcNow:yyyyMMdd}.txt";
            var filePath = Path.Combine(_logsFolder, fileName);

            await File.AppendAllTextAsync(filePath, logEntry, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

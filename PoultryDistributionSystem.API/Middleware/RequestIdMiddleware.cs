namespace PoultryDistributionSystem.API.Middleware;

/// <summary>
/// Middleware to ensure request ID is set for logging
/// </summary>
public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Ensure TraceIdentifier is set (used as RequestId)
        if (string.IsNullOrEmpty(context.TraceIdentifier))
        {
            context.TraceIdentifier = Guid.NewGuid().ToString();
        }

        // Add RequestId header to response
        context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;

        await _next(context);
    }
}

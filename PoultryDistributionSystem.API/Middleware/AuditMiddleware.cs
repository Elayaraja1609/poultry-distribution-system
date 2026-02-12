using System.Text.Json;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Enums;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Middleware;

/// <summary>
/// Middleware to automatically log API actions
/// </summary>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // Skip audit for GET requests and certain endpoints
        if (context.Request.Method == "GET" || 
            context.Request.Path.Value?.Contains("/swagger") == true ||
            context.Request.Path.Value?.Contains("/api/auth/login") == true)
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var userId = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var guid) ? guid : (Guid?)null;

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        // Try to determine entity type and ID from route
        var routeData = context.Request.RouteValues;
        var entityType = routeData.ContainsKey("controller") ? routeData["controller"]?.ToString() : null;
        var entityId = routeData.ContainsKey("id") && Guid.TryParse(routeData["id"]?.ToString(), out var id) ? id : Guid.Empty;

        if (entityType != null && entityId != Guid.Empty)
        {
            var action = context.Request.Method switch
            {
                "POST" => AuditAction.Create,
                "PUT" => AuditAction.Update,
                "DELETE" => AuditAction.Delete,
                _ => AuditAction.Update
            };

            try
            {
                await auditService.LogActionAsync(
                    entityType,
                    entityId,
                    action,
                    userId,
                    null, // Old values - could be extracted from request body
                    null, // New values - could be extracted from request body
                    ipAddress,
                    context.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit entry");
                // Don't fail the request if audit logging fails
            }
        }

        await _next(context);
    }
}

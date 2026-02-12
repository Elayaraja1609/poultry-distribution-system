using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.API.Middleware;

/// <summary>
/// Middleware for tenant isolation in multi-tenant SaaS
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        // Extract tenant identifier from subdomain or header
        var host = context.Request.Host.Host;
        var subdomain = host.Split('.').FirstOrDefault();
        
        // Or get from header
        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        Guid? tenantId = null;

        if (!string.IsNullOrEmpty(tenantHeader) && Guid.TryParse(tenantHeader, out var tenantGuid))
        {
            tenantId = tenantGuid;
        }
        else if (!string.IsNullOrEmpty(subdomain) && subdomain != "localhost" && subdomain != "api")
        {
            // Look up tenant by subdomain
            var tenants = await unitOfWork.Tenants.FindAsync(
                t => t.Subdomain == subdomain && t.IsActive && !t.IsDeleted,
                context.RequestAborted);
            var tenant = tenants.FirstOrDefault();
            tenantId = tenant?.Id;
        }

        // Store tenant ID in HttpContext for use in services
        if (tenantId.HasValue)
        {
            context.Items["TenantId"] = tenantId.Value;
        }

        await _next(context);
    }
}

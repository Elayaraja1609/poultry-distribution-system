namespace PoultryDistributionSystem.Application.DTOs.Tenant;

/// <summary>
/// Tenant DTO
/// </summary>
public class TenantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public int MaxUsers { get; set; }
    public int MaxShops { get; set; }
    public int MaxFarms { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Create tenant DTO
/// </summary>
public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = "Basic";
}

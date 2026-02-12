using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Tenant entity for multi-tenant SaaS support
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public string SubscriptionPlan { get; set; } = "Basic"; // Basic, Professional, Enterprise
    public int MaxUsers { get; set; } = 10;
    public int MaxShops { get; set; } = 5;
    public int MaxFarms { get; set; } = 3;
    public string? Settings { get; set; } // JSON settings

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}

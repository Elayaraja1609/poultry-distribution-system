using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Permission entity for fine-grained access control
/// </summary>
public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // e.g., 'suppliers', 'farms', 'chickens'
    public string Action { get; set; } = string.Empty; // e.g., 'create', 'read', 'update', 'delete'
    public string Description { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

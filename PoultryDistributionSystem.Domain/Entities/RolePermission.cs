using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Junction entity for role-permission relationships
/// </summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}

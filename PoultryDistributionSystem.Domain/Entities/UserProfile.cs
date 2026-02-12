using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// User profile entity for additional user information
/// </summary>
public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}

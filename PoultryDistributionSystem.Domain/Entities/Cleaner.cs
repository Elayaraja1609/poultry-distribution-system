using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Cleaner entity representing vehicle cleaners responsible for loading/unloading
/// </summary>
public class Cleaner : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}

using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Farm entity representing storage and growing facilities
/// </summary>
public class Farm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentCount { get; set; } = 0;
    public string ManagerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Chicken> Chickens { get; set; } = new List<Chicken>();
    public virtual ICollection<FarmOperation> FarmOperations { get; set; } = new List<FarmOperation>();
}

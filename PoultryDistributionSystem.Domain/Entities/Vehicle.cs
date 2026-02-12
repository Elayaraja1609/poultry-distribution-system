using PoultryDistributionSystem.Domain.Common;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Vehicle entity representing distribution vehicles
/// </summary>
public class Vehicle : BaseEntity
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Guid DriverId { get; set; }
    public Guid CleanerId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
    public virtual Cleaner Cleaner { get; set; } = null!;
    public virtual ICollection<Distribution> Distributions { get; set; } = new List<Distribution>();
}

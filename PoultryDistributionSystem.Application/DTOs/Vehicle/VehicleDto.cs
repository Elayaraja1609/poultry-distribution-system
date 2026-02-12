namespace PoultryDistributionSystem.Application.DTOs.Vehicle;

/// <summary>
/// Vehicle DTO
/// </summary>
public class VehicleDto
{
    public Guid Id { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Guid DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public Guid CleanerId { get; set; }
    public string CleanerName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

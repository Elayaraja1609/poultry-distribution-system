namespace PoultryDistributionSystem.Application.DTOs.Vehicle;

/// <summary>
/// Create vehicle request DTO
/// </summary>
public class CreateVehicleDto
{
    public string VehicleNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Guid DriverId { get; set; }
    public Guid CleanerId { get; set; }
}

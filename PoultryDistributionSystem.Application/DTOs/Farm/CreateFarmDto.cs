namespace PoultryDistributionSystem.Application.DTOs.Farm;

/// <summary>
/// Create farm request DTO
/// </summary>
public class CreateFarmDto
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

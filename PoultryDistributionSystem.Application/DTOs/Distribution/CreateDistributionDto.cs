namespace PoultryDistributionSystem.Application.DTOs.Distribution;

/// <summary>
/// Create distribution request DTO
/// </summary>
public class CreateDistributionDto
{
    public Guid VehicleId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public List<DistributionItemDto> Items { get; set; } = new();
}

/// <summary>
/// Distribution item DTO
/// </summary>
public class DistributionItemDto
{
    public Guid ChickenId { get; set; }
    public Guid ShopId { get; set; }
    public int Quantity { get; set; }
}

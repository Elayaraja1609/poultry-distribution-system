namespace PoultryDistributionSystem.Application.DTOs.Chicken;

/// <summary>
/// Create chicken request DTO
/// </summary>
public class CreateChickenDto
{
    public string BatchNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid? FarmId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int Quantity { get; set; }
    public decimal WeightKg { get; set; }
}

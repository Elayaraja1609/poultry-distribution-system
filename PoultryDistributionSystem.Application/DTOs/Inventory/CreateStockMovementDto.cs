using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Inventory;

/// <summary>
/// Create stock movement DTO
/// </summary>
public class CreateStockMovementDto
{
    public Guid FarmId { get; set; }
    public Guid ChickenId { get; set; }
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public DateTime? MovementDate { get; set; }
}

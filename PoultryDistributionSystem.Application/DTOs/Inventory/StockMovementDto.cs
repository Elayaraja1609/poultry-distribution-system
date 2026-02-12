using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Inventory;

/// <summary>
/// Stock movement DTO
/// </summary>
public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid FarmId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public Guid ChickenId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public string? Reason { get; set; }
    public DateTime MovementDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

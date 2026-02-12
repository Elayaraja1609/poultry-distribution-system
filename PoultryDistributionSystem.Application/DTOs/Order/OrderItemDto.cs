namespace PoultryDistributionSystem.Application.DTOs.Order;

/// <summary>
/// Order item DTO
/// </summary>
public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ChickenId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int FulfilledQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

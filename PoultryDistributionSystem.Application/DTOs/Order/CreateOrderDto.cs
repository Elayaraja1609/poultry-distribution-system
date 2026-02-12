namespace PoultryDistributionSystem.Application.DTOs.Order;

/// <summary>
/// Create order request DTO
/// </summary>
public class CreateOrderDto
{
    public DateTime RequestedDeliveryDate { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Create order item DTO
/// </summary>
public class CreateOrderItemDto
{
    public Guid ChickenId { get; set; }
    public int Quantity { get; set; }
}

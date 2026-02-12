namespace PoultryDistributionSystem.Application.DTOs.Order;

/// <summary>
/// Update fulfillment DTO
/// </summary>
public class UpdateFulfillmentDto
{
    public List<UpdateFulfillmentItemDto> Items { get; set; } = new();
}

/// <summary>
/// Update fulfillment item DTO
/// </summary>
public class UpdateFulfillmentItemDto
{
    public Guid OrderItemId { get; set; }
    public int FulfilledQuantity { get; set; }
}

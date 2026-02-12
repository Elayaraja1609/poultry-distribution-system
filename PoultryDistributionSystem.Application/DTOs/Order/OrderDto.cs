using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Order;

/// <summary>
/// Order DTO
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime RequestedDeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }
    public FulfillmentStatus FulfillmentStatus { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

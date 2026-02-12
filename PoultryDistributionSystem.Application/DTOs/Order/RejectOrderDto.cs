namespace PoultryDistributionSystem.Application.DTOs.Order;

/// <summary>
/// Reject order request DTO
/// </summary>
public class RejectOrderDto
{
    public string Reason { get; set; } = string.Empty;
}

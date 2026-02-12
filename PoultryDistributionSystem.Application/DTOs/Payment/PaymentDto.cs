using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Payment;

/// <summary>
/// Payment DTO
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentGatewayTransactionId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

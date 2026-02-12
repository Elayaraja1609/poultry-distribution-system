using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Payment;

/// <summary>
/// Create payment request DTO
/// </summary>
public class CreatePaymentDto
{
    public Guid SaleId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentGatewayTransactionId { get; set; }
    public string? Notes { get; set; }
}

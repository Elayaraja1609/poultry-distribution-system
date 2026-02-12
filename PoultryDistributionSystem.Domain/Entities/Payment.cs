using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Payment entity representing individual payments for sales
/// </summary>
public class Payment : BaseEntity
{
    public Guid SaleId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentGatewayTransactionId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Sale Sale { get; set; } = null!;
}

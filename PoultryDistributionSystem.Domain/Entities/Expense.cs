using PoultryDistributionSystem.Domain.Common;
using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Domain.Entities;

/// <summary>
/// Expense entity for tracking business expenses
/// </summary>
public class Expense : BaseEntity
{
    public ExpenseType ExpenseType { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public Guid? VehicleId { get; set; }
    public Guid? FarmId { get; set; }
    public string? ReceiptPath { get; set; }

    // Navigation properties
    public virtual Vehicle? Vehicle { get; set; }
    public virtual Farm? Farm { get; set; }
}

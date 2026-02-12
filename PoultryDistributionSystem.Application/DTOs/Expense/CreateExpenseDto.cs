using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Expense;

/// <summary>
/// Create expense DTO
/// </summary>
public class CreateExpenseDto
{
    public ExpenseType ExpenseType { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public Guid? VehicleId { get; set; }
    public Guid? FarmId { get; set; }
    public string? ReceiptPath { get; set; }
}

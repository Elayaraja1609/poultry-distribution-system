using PoultryDistributionSystem.Domain.Enums;

namespace PoultryDistributionSystem.Application.DTOs.Expense;

/// <summary>
/// Expense DTO
/// </summary>
public class ExpenseDto
{
    public Guid Id { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public Guid? VehicleId { get; set; }
    public string? VehicleNumber { get; set; }
    public Guid? FarmId { get; set; }
    public string? FarmName { get; set; }
    public string? ReceiptPath { get; set; }
    public DateTime CreatedAt { get; set; }
}

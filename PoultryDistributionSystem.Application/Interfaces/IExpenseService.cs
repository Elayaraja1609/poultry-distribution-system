using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Expense;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Expense service interface
/// </summary>
public interface IExpenseService
{
    Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ExpenseDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ExpenseDto>> GetByCategoryAsync(string category, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ExpenseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<ExpenseDto> UpdateAsync(Guid id, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalExpensesAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}

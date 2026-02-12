using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Expense;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Expense service implementation
/// </summary>
public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ExpenseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense == null)
        {
            throw new KeyNotFoundException($"Expense with ID {id} not found");
        }

        var dto = _mapper.Map<ExpenseDto>(expense);

        if (expense.VehicleId.HasValue)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(expense.VehicleId.Value, cancellationToken);
            dto.VehicleNumber = vehicle?.VehicleNumber;
        }

        if (expense.FarmId.HasValue)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(expense.FarmId.Value, cancellationToken);
            dto.FarmName = farm?.Name;
        }

        return dto;
    }

    public async Task<PagedResult<ExpenseDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allExpenses = await _unitOfWork.Expenses.FindAsync(e => !e.IsDeleted, cancellationToken);
        var expensesList = allExpenses.OrderByDescending(e => e.ExpenseDate).ToList();

        var totalCount = expensesList.Count;
        var pagedExpenses = expensesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<ExpenseDto>();
        foreach (var expense in pagedExpenses)
        {
            var dto = _mapper.Map<ExpenseDto>(expense);

            if (expense.VehicleId.HasValue)
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(expense.VehicleId.Value, cancellationToken);
                dto.VehicleNumber = vehicle?.VehicleNumber;
            }

            if (expense.FarmId.HasValue)
            {
                var farm = await _unitOfWork.Farms.GetByIdAsync(expense.FarmId.Value, cancellationToken);
                dto.FarmName = farm?.Name;
            }

            items.Add(dto);
        }

        return new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ExpenseDto>> GetByCategoryAsync(string category, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allExpenses = await _unitOfWork.Expenses.FindAsync(
            e => e.Category == category && !e.IsDeleted,
            cancellationToken);
        var expensesList = allExpenses.OrderByDescending(e => e.ExpenseDate).ToList();

        var totalCount = expensesList.Count;
        var pagedExpenses = expensesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<ExpenseDto>();
        foreach (var expense in pagedExpenses)
        {
            var dto = _mapper.Map<ExpenseDto>(expense);
            if (expense.VehicleId.HasValue)
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(expense.VehicleId.Value, cancellationToken);
                dto.VehicleNumber = vehicle?.VehicleNumber;
            }
            if (expense.FarmId.HasValue)
            {
                var farm = await _unitOfWork.Farms.GetByIdAsync(expense.FarmId.Value, cancellationToken);
                dto.FarmName = farm?.Name;
            }
            items.Add(dto);
        }

        return new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ExpenseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allExpenses = await _unitOfWork.Expenses.FindAsync(
            e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate && !e.IsDeleted,
            cancellationToken);
        var expensesList = allExpenses.OrderByDescending(e => e.ExpenseDate).ToList();

        var totalCount = expensesList.Count;
        var pagedExpenses = expensesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<ExpenseDto>();
        foreach (var expense in pagedExpenses)
        {
            var dto = _mapper.Map<ExpenseDto>(expense);
            if (expense.VehicleId.HasValue)
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(expense.VehicleId.Value, cancellationToken);
                dto.VehicleNumber = vehicle?.VehicleNumber;
            }
            if (expense.FarmId.HasValue)
            {
                var farm = await _unitOfWork.Farms.GetByIdAsync(expense.FarmId.Value, cancellationToken);
                dto.FarmName = farm?.Name;
            }
            items.Add(dto);
        }

        return new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var expense = _mapper.Map<Expense>(dto);
        expense.CreatedBy = createdBy;

        await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(expense.Id, cancellationToken);
    }

    public async Task<ExpenseDto> UpdateAsync(Guid id, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense == null)
        {
            throw new KeyNotFoundException($"Expense with ID {id} not found");
        }

        expense.ExpenseType = dto.ExpenseType;
        expense.Category = dto.Category;
        expense.Amount = dto.Amount;
        expense.Description = dto.Description;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.VehicleId = dto.VehicleId;
        expense.FarmId = dto.FarmId;
        expense.ReceiptPath = dto.ReceiptPath;
        expense.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Expenses.UpdateAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense == null)
        {
            return false;
        }

        await _unitOfWork.Expenses.DeleteAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<decimal> GetTotalExpensesAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var allExpenses = await _unitOfWork.Expenses.FindAsync(e => !e.IsDeleted, cancellationToken);

        if (startDate.HasValue)
        {
            allExpenses = allExpenses.Where(e => e.ExpenseDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            allExpenses = allExpenses.Where(e => e.ExpenseDate <= endDate.Value);
        }

        return allExpenses.Sum(e => e.Amount);
    }

    public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var allExpenses = await _unitOfWork.Expenses.FindAsync(e => !e.IsDeleted, cancellationToken);

        if (startDate.HasValue)
        {
            allExpenses = allExpenses.Where(e => e.ExpenseDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            allExpenses = allExpenses.Where(e => e.ExpenseDate <= endDate.Value);
        }

        return allExpenses
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }
}

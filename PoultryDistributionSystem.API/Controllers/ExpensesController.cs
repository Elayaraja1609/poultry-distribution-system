using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Expense;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using Infrastructure = PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Expenses controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class ExpensesController : ControllerBase
{
    private readonly IExpenseService _expenseService;
    private readonly PoultryDistributionSystem.Infrastructure.Services.Interfaces.IPdfService _pdfService;

    public ExpensesController(IExpenseService expenseService, PoultryDistributionSystem.Infrastructure.Services.Interfaces.IPdfService pdfService)
    {
        _expenseService = expenseService ?? throw new ArgumentNullException(nameof(expenseService));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _expenseService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<ExpenseDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _expenseService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpenseDto>>.SuccessResponse(result));
    }

    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseDto>>>> GetByCategory(
        string category,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _expenseService.GetByCategoryAsync(category, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpenseDto>>.SuccessResponse(result));
    }

    [HttpGet("date-range")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseDto>>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _expenseService.GetByDateRangeAsync(startDate, endDate, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpenseDto>>.SuccessResponse(result));
    }

    [HttpGet("total")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetTotal(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _expenseService.GetTotalExpensesAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<decimal>.SuccessResponse(result));
    }

    [HttpGet("by-category")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, decimal>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, decimal>>>> GetByCategory(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _expenseService.GetExpensesByCategoryAsync(startDate, endDate, cancellationToken);
        return Ok(ApiResponse<Dictionary<string, decimal>>.SuccessResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Create([FromBody] CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _expenseService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ExpenseDto>.SuccessResponse(result, "Expense created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> Update(Guid id, [FromBody] CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _expenseService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<ExpenseDto>.SuccessResponse(result, "Expense updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _expenseService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Expense not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Expense deleted successfully"));
    }

    [HttpGet("{id}/receipt")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceipt(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfService.GenerateExpenseReceiptAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"expense-receipt-{id.ToString().Substring(0, 8)}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

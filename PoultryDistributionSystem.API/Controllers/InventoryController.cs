using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Inventory;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Inventory controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,FarmManager")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
    }

    [HttpGet("farms/{farmId}")]
    [ProducesResponseType(typeof(ApiResponse<FarmInventoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FarmInventoryDto>>> GetFarmInventory(Guid farmId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inventoryService.GetFarmInventoryAsync(farmId, cancellationToken);
            return Ok(ApiResponse<FarmInventoryDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("movements")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<StockMovementDto>>>> GetStockMovements(
        [FromQuery] Guid? farmId,
        [FromQuery] Guid? chickenId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _inventoryService.GetStockMovementsAsync(farmId, chickenId, startDate, endDate, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<StockMovementDto>>.SuccessResponse(result));
    }

    [HttpPost("movements")]
    [ProducesResponseType(typeof(ApiResponse<StockMovementDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<StockMovementDto>>> RecordStockMovement(
        [FromBody] CreateStockMovementDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _inventoryService.RecordStockMovementAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetStockMovements), new { id = result.Id }, ApiResponse<StockMovementDto>.SuccessResponse(result, "Stock movement recorded successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("farms/{farmId}/stock-summary")]
    [ProducesResponseType(typeof(ApiResponse<StockSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StockSummaryDto>>> GetStockSummary(
        Guid farmId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inventoryService.GetStockSummaryAsync(farmId, startDate, endDate, cancellationToken);
            return Ok(ApiResponse<StockSummaryDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

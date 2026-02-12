using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Delivery;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Deliveries controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,Driver,ShopOwner")]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeliveryDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _deliveryService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<DeliveryDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeliveryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeliveryDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<DeliveryDto>>.SuccessResponse(result));
    }

    [HttpGet("shop/{shopId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeliveryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeliveryDto>>>> GetByShop(
        Guid shopId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _deliveryService.GetByShopIdAsync(shopId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<DeliveryDto>>.SuccessResponse(result));
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DeliveryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<DeliveryDto>>>> GetMyDeliveries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.GetMyDeliveriesAsync(userId, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<DeliveryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DeliveryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DeliveryDto>>> Update(Guid id, [FromBody] UpdateDeliveryDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _deliveryService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<DeliveryDto>.SuccessResponse(result, "Delivery updated successfully"));
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

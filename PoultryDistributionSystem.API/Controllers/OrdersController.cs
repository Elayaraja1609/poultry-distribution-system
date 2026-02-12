using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Order;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Orders controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,ShopOwner")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetOrders(
        [FromQuery] Guid? shopId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _orderService.GetOrdersAsync(shopId, status, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<OrderDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [Authorize(Roles = "ShopOwner")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _orderService.CreateOrderAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, ApiResponse<OrderDto>.SuccessResponse(result, "Order created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ApproveOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var approvedBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _orderService.ApproveOrderAsync(id, approvedBy, cancellationToken);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result, "Order approved successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> RejectOrder(
        Guid id,
        [FromBody] RejectOrderDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var rejectedBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _orderService.RejectOrderAsync(id, dto.Reason, rejectedBy, cancellationToken);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result, "Order rejected successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/fulfillment")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateFulfillment(
        Guid id,
        [FromBody] UpdateFulfillmentDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _orderService.UpdateFulfillmentAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result, "Fulfillment updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> GetMyOrders(
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

            var result = await _orderService.GetMyOrdersAsync(userId, pageNumber, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<OrderDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var cancelledBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _orderService.CancelOrderAsync(id, cancelledBy, cancellationToken);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(result, "Order cancelled successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Shop;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Shops controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class ShopsController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopsController(IShopService shopService)
    {
        _shopService = shopService ?? throw new ArgumentNullException(nameof(shopService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ShopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ShopDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _shopService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<ShopDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ShopDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ShopDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _shopService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ShopDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ShopDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ShopDto>>> Create([FromBody] CreateShopDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _shopService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ShopDto>.SuccessResponse(result, "Shop created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ShopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ShopDto>>> Update(Guid id, [FromBody] CreateShopDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _shopService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<ShopDto>.SuccessResponse(result, "Shop updated successfully"));
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
        var result = await _shopService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Shop not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Shop deleted successfully"));
    }
}

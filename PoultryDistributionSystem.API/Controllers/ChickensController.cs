using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Chicken;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Chickens controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,FarmManager")]
public class ChickensController : ControllerBase
{
    private readonly IChickenService _chickenService;

    public ChickensController(IChickenService chickenService)
    {
        _chickenService = chickenService ?? throw new ArgumentNullException(nameof(chickenService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ChickenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChickenDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _chickenService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<ChickenDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ChickenDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ChickenDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _chickenService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ChickenDto>>.SuccessResponse(result));
    }

    [HttpGet("farm/{farmId}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ChickenDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ChickenDto>>>> GetByFarm(
        Guid farmId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _chickenService.GetByFarmIdAsync(farmId, pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ChickenDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ChickenDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ChickenDto>>> Create([FromBody] CreateChickenDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _chickenService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ChickenDto>.SuccessResponse(result, "Chicken batch created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ChickenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChickenDto>>> Update(Guid id, [FromBody] UpdateChickenDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _chickenService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<ChickenDto>.SuccessResponse(result, "Chicken updated successfully"));
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _chickenService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Chicken not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Chicken deleted successfully"));
    }
}

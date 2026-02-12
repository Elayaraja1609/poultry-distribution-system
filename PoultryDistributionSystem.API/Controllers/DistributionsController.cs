using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Distribution;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Enums;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Distributions controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin,Driver")]
public class DistributionsController : ControllerBase
{
    private readonly IDistributionService _distributionService;

    public DistributionsController(IDistributionService distributionService)
    {
        _distributionService = distributionService ?? throw new ArgumentNullException(nameof(distributionService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DistributionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DistributionDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _distributionService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<DistributionDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DistributionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<DistributionDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _distributionService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<DistributionDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DistributionDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<DistributionDto>>> Create([FromBody] CreateDistributionDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _distributionService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DistributionDto>.SuccessResponse(result, "Distribution created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<DistributionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DistributionDto>>> UpdateStatus(Guid id, [FromBody] DistributionStatus status, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _distributionService.UpdateStatusAsync(id, status, cancellationToken);
            return Ok(ApiResponse<DistributionDto>.SuccessResponse(result, "Distribution status updated successfully"));
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
        var result = await _distributionService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Distribution not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Distribution deleted successfully"));
    }
}

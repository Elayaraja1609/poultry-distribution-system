using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Cleaner;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Cleaners controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class CleanersController : ControllerBase
{
    private readonly ICleanerService _cleanerService;

    public CleanersController(ICleanerService cleanerService)
    {
        _cleanerService = cleanerService ?? throw new ArgumentNullException(nameof(cleanerService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CleanerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CleanerDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _cleanerService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<CleanerDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CleanerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<CleanerDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _cleanerService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<CleanerDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CleanerDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CleanerDto>>> Create([FromBody] CreateCleanerDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _cleanerService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<CleanerDto>.SuccessResponse(result, "Cleaner created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CleanerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CleanerDto>>> Update(Guid id, [FromBody] CreateCleanerDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _cleanerService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<CleanerDto>.SuccessResponse(result, "Cleaner updated successfully"));
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
        var result = await _cleanerService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Cleaner not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Cleaner deleted successfully"));
    }
}

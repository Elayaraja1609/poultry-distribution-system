using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Driver;
using PoultryDistributionSystem.Application.Interfaces;
using System.Security.Claims;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Drivers controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class DriversController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversController(IDriverService driverService)
    {
        _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DriverDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _driverService.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<DriverDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DriverDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<DriverDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _driverService.GetAllAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<DriverDto>>.SuccessResponse(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DriverDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<DriverDto>>> Create([FromBody] CreateDriverDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var createdBy = userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;

            var result = await _driverService.CreateAsync(dto, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DriverDto>.SuccessResponse(result, "Driver created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DriverDto>>> Update(Guid id, [FromBody] CreateDriverDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _driverService.UpdateAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<DriverDto>.SuccessResponse(result, "Driver updated successfully"));
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
        var result = await _driverService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Driver not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Driver deleted successfully"));
    }
}

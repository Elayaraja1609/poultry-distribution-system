using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Tenant;
using PoultryDistributionSystem.Application.Interfaces;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Tenants controller (Super Admin only)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")] // In production, use "SuperAdmin" role
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TenantDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<TenantDto>>>> GetAllTenants(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _tenantService.GetAllTenantsAsync(pageNumber, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<TenantDto>>.SuccessResponse(result));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> GetTenantById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tenantService.GetTenantByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<TenantDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("subdomain/{subdomain}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> GetTenantBySubdomain(string subdomain, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tenantService.GetTenantBySubdomainAsync(subdomain, cancellationToken);
            return Ok(ApiResponse<TenantDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> CreateTenant(
        [FromBody] CreateTenantDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tenantService.CreateTenantAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetTenantById), new { id = result.Id }, ApiResponse<TenantDto>.SuccessResponse(result, "Tenant created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TenantDto>>> UpdateTenant(
        Guid id,
        [FromBody] CreateTenantDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tenantService.UpdateTenantAsync(id, dto, cancellationToken);
            return Ok(ApiResponse<TenantDto>.SuccessResponse(result, "Tenant updated successfully"));
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
    public async Task<ActionResult<ApiResponse<object>>> DeleteTenant(Guid id, CancellationToken cancellationToken)
    {
        var result = await _tenantService.DeleteTenantAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Tenant not found"));
        }

        return Ok(ApiResponse<object?>.SuccessResponse(null, "Tenant deleted successfully"));
    }
}

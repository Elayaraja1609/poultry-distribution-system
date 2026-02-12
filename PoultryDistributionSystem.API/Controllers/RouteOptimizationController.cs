using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Route;
using PoultryDistributionSystem.Application.Interfaces;

namespace PoultryDistributionSystem.API.Controllers;

/// <summary>
/// Route optimization controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
//[Authorize(Roles = "Admin")]
public class RouteOptimizationController : ControllerBase
{
    private readonly IRouteOptimizationService _routeOptimizationService;

    public RouteOptimizationController(IRouteOptimizationService routeOptimizationService)
    {
        _routeOptimizationService = routeOptimizationService ?? throw new ArgumentNullException(nameof(routeOptimizationService));
    }

    [HttpPost("optimize")]
    [ProducesResponseType(typeof(ApiResponse<RouteOptimizationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RouteOptimizationDto>>> OptimizeRoute(
        [FromBody] RouteOptimizationRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _routeOptimizationService.OptimizeDeliveryRouteAsync(request, cancellationToken);
            return Ok(ApiResponse<RouteOptimizationDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("distance")]
    [ProducesResponseType(typeof(ApiResponse<double>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<double>>> CalculateDistance(
        [FromQuery] string address1,
        [FromQuery] string address2,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _routeOptimizationService.CalculateDistanceAsync(address1, address2, cancellationToken);
            return Ok(ApiResponse<double>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
